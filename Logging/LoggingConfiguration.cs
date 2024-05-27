using Microsoft.Extensions.Configuration;
using Serilog;

namespace AmxBookstore.Infrastructure.Logging
{
    public static class LoggingConfiguration
    {
        public static void ConfigureLogging(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
