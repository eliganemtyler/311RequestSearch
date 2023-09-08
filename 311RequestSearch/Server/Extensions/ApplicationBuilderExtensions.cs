using System;
using IdentityModel.Client;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;

namespace _311RequestSearch.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomSwaggerApi(this IApplicationBuilder app)
        {
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/swagger";
                settings.SwaggerRoutes.Add(new SwaggerUi3Route("v1", $"/swagger/v1/swagger.json"));

            });

            return app;
        }
        // Configure XSRF middleware, This pattern is for SPA style applications where XSRF token is added on Index page
        // load and passed back token on every subsequent async request
        public static IApplicationBuilder UseXsrf(this IApplicationBuilder app)
        {
            var antiforgery = app.ApplicationServices.GetRequiredService<IAntiforgery>();

            app.Use(async (context, next) =>
            {
                if (string.Equals(context.Request.Path.Value, "/", StringComparison.OrdinalIgnoreCase))
                {
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                }
                await next.Invoke();
            });

            return app;
        }
        public static IApplicationBuilder AddDevMiddlewares(this IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            // NOTE: For SPA swagger needs adding before MVC
            app.UseCustomSwaggerApi();
            return app;
        }

        public static IApplicationBuilder UseTcpCookiePolicy(this IApplicationBuilder app, IConfiguration configuration)
        {
            var envDomainProtocol = configuration.GetValue<string>("EnvironmentDomainProtocol");
            var useHttp = envDomainProtocol == "http" ? true : false;
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = useHttp ? SameSiteMode.Lax : SameSiteMode.None,
                Secure = useHttp ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
            });
            return app;
        }
    }
}
