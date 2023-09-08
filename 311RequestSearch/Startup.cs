using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using _311RequestSearch.Server;
using _311RequestSearch.Server.Configuration;
using _311RequestSearch.Server.Entities.Config;
using _311RequestSearch.Server.Entities.Helper;
using _311RequestSearch.Server.Extensions;
using _311RequestSearch.Server.Extensions.Spa;
using _311RequestSearch.Server.Startup;
using Tyler.Extensions.Authentication;
using Tyler.Platform.SDK.DependencyInjection;
using Yarp.ReverseProxy.Forwarder;
using System.Net.Http;
using System;
using System.Net;
using System.Diagnostics;

namespace _311RequestSearch
{
    public class Startup
    {
        static string title = "311requestsearch";
        static readonly string spaRootPath = "spa";
        readonly SupportedLanguages supportedLanguages = new SupportedLanguages(new []{"en", "es-MX"}, spaRootPath);

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; set; }
        private IWebHostEnvironment _env { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<ServerAppConfig>(Configuration)
                .Configure<ClientAppConfig>(Configuration)
                .AddOptions()
                .AddCors(options =>
                {
                    options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
                })
                .AddResponseCompression(options =>
                {
                    options.MimeTypes = DefaultMimeTypes.Get;
                })
                .AddMemoryCache()
                .RegisterCustomServices()
                .AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN")
                .AddCustomizedMvc()
                .RegisterTylerCacheServices(Configuration)
                .AddSupportedLanguagesService(supportedLanguages);

            services.AddSwaggerDocument(config => {
                config.Title = title;
            })
            .AddHttpClient()
            .AddHttpContextAccessor();

            // Add Tyler Cloud Platform services
            InitPlatformSdk(services);
            services.AddScoped<ITenant, Tenant>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add dyanmic auth
            services.AddTcpAuthentication(Configuration);
            services.AddTidIdentityConfigurationService(Configuration);

            services.AddHttpForwarder();

            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime, IHttpForwarder forwarder)
        {
            // IMPORTANT NOTE: rootPath is the base href.  This value must match what is set
            // for <base href="<baseHref>"> in client/src/index.html.
            //
            // At this time there is no easy way to set a string that is shared
            // accross the client/server boundary, because the client build process
            // generates a static application.  If this value existed in appsettings.json
            // it would imply it could be changed at run-time, when it cannot be updated
            // at runtime in the client application.
            //
            // This value can be accessed from the static function Context.GetBaseHref();
            // Uri strings that use the {baseHref} string token can be substituted using
            // the UrlAdjuster.ReplaceHostAndBaseHref() method.
            var rootPath = new PathString("/app/new-application");

            if (env.IsProduction())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
                app.UseResponseCompression();
            }

            app.UsePathBase(rootPath);

            if (env.IsDevelopment())
            {
                app.AddDevMiddlewares();
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                },
            });

            app.UseTcpCookiePolicy(Configuration);

            app.UseXsrf()
            .UseCors("AllowAll");

            app.UseAuthentication();
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseOpenApi();

            app.UseRouting();
            app.UseAuthorization();

            // https://microsoft.github.io/reverse-proxy/articles/direct-forwarding.html#the-http-client
            var proxyHttpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // add fallback for api routes
                endpoints.Map("/api/{**route}", request => { request.Response.StatusCode = 404; return System.Threading.Tasks.Task.CompletedTask; });

                endpoints.MapHealthChecks("/health").AllowAnonymous();

                // https://microsoft.github.io/reverse-proxy/articles/direct-forwarding.html
                endpoints.Map("/px/branding/{**route}", async httpContext =>
                    await forwarder.SendAsync(
                        httpContext, 
                        $"{UrlAdjuster.ReplaceTokensAndSetTrailingSlash(Configuration.GetValue<string>("brandingServiceUri"))}",
                        proxyHttpClient,
                        new ForwarderRequestConfig {ActivityTimeout = TimeSpan.FromSeconds(100)},
                        new BrandingProxyTransformer("route")));
            });

            app.UseTcpAuthentication();

            var ignoreSpaDevServerMiddleware = Configuration.GetValue<bool>("ignoreSpaDevServerMiddleware");
            app.UseLocalizedSpa(env, supportedLanguages, ignoreSpaDevServerMiddleware);

            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Context.Configure(httpContextAccessor, rootPath.Value, Configuration);
        }

        private void InitPlatformSdk(IServiceCollection services)
        {
            services.AddPlatformSdk(options => {
                options.AuthTokenUri = Configuration["oauth2:tcp_platform:authority"];
                options.TokenClientId = Configuration["oauth2:tcp_platform:clientId"];
                options.TokenClientSecret = Configuration["oauth2:tcp_platform:clientSecret"];
                options.TokenClientScope = "PlatformServiceSDKClients";
            }, Configuration);
        }
    }
}
