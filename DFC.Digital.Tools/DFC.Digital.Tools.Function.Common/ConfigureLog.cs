using DFC.Digital.Tools.Core;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using System.IO;

namespace DFC.Digital.Tools.Function.Common
{
    public static class ConfigureLog
    {
        public static void ConfigureNLogWithAppInsightsTarget()
        {
            // TO BE fIXED
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var appInsightsKey = configuration[Constants.ApplicationInsightsInstrumentationKey];
            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                var config = new LoggingConfiguration();

                ApplicationInsightsTarget target = new ApplicationInsightsTarget();
                target.InstrumentationKey = appInsightsKey;

                LoggingRule rule = new LoggingRule("*", LogLevel.Trace, target);
                config.LoggingRules.Add(rule);

                LogManager.Configuration = config;
            }
        }
    }
}
