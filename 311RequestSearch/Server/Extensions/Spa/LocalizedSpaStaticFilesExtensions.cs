using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _311RequestSearch.Server.Extensions.Spa
{
  public static class LocalizedSpaStaticFilesExtensions
  {
    public static void AddSupportedLanguagesService(this IServiceCollection services, SupportedLanguages supportedLanguages)
    {
      services.AddScoped<IUserLanguageService>(serviceProvider =>
      {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        return new UserLanguageService(httpContextAccessor, supportedLanguages);
      });

      // services.AddSpaStaticFiles(configuration => configuration.RootPath = "spa/en");
    }

    public static void UseLocalizedSpa(this IApplicationBuilder app, IWebHostEnvironment env, SupportedLanguages supportedLanguages, bool ignoreSpaDevServer)
    {  
      if(env.IsProduction() || ignoreSpaDevServer)
      {
        // setup for all languages but the default
        foreach(var lang in supportedLanguages.Languages.Where(lang => lang != supportedLanguages.DefaultLanguage))
        {
          app.UseWhen(
            context => {
              var userLanguageService = context.RequestServices.GetRequiredService<IUserLanguageService>();
              var userLocale = userLanguageService.GetUserLocale();
              return lang == userLocale;
            },
            builder => {
              SetupSpa(builder, supportedLanguages.LangSpaPathMap[lang]);
            }          
          );
        }

        // setup default language      
        var defaultLangPath = supportedLanguages.LangSpaPathMap[supportedLanguages.DefaultLanguage];
        SetupSpa(app, defaultLangPath);
      }
      else
      {
        app.UseSpa(spa =>
        {
          spa.Options.SourcePath = "client";
          
          var clientPort = Environment.GetEnvironmentVariable("CLIENT_DEV_SERVER_PORT") ?? "4200";
          spa.UseProxyToSpaDevelopmentServer($"http://localhost:{clientPort}");
        });
      }
    }

    private static void SetupSpa(IApplicationBuilder app, string langPath)
    {
      var staticFileOptions = new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), langPath))
      };
      app.UseSpaStaticFiles(staticFileOptions);
      app.UseSpa(spa =>
      {
        spa.Options.SourcePath = "client";
        spa.Options.DefaultPageStaticFileOptions = staticFileOptions;
      });
    }
  }
}
