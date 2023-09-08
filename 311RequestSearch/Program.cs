using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using _311RequestSearch.Server;
using _311RequestSearch.Server.Extensions;
using Tyler.Extensions.Configuration.Consul;
using Microsoft.Extensions.Hosting;

namespace _311RequestSearch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cross-platform way to set the machine name environment variable.
            // Serilog will only look for HOSTNAME which is not guaranteed to exist
            // on any platform.  With this call, we're making that guarantee.
            Environment.SetEnvironmentVariable("HOSTNAME", System.Environment.MachineName);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => {
                    var appsettingsConfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddEnvironmentVariables()
                        .Build();

                    // Load settings from Consul if they have specified a configuration server
                    // Throw an exception if we cannot connect successfully after several retries
                    if (ConsulConnection.TryConnect(appsettingsConfig)) {
                        config
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddConsulServer(appsettingsConfig, "environment.json")
                            .AddConsulServer(appsettingsConfig, "311RequestSearch/appsettings.json")
                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
                    }
                    else
                    {
                        config
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
                    }

                    if (hostingContext.HostingEnvironment.IsDevelopment()) {
                        // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                        config.AddUserSecrets<Startup>(true);
                    }
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.ClearProviders();
                    Server.Startup.Serilog.Setup(hostingContext);
                    logging.AddSerilog(dispose: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(
                        new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddEnvironmentVariables()
                            .AddCommandLine(args)
                            .Build()
                    )
                    .UseStartup<Startup>();
                });
    }
}
