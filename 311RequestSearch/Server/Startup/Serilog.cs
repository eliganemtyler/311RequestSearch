using Serilog;
using Microsoft.Extensions.Hosting;

namespace _311RequestSearch.Server.Startup
{
    public static class Serilog
    {
        public static void Setup(HostBuilderContext hostingContext)
        {
            var log = new LoggerConfiguration().ReadFrom.Configuration(hostingContext.Configuration);
            Log.Logger = log.CreateLogger();
        }
    }
}