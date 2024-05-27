using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AmxBookstore.Infrastructure.Logging
{
    public static class SerilogLogger
    {
        public static void AddSerilogLogging(this IServiceCollection services)
        {
            services.AddSingleton(Log.Logger);
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));
        }
    }
}
