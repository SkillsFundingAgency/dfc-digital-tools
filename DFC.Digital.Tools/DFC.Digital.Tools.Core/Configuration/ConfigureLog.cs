using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog;
using NLog.Config;
using System.IO;

namespace DFC.Digital.Tools.Core
{
    public static class ConfigureLog
    {
        public static void ConfigureNLogWithAppInsightsTarget(IConfigConfigurationProvider configProvider)
        {
            var appInsightsKey = configProvider.GetConfig<string>(Constants.ApplicationInsightsInstrumentationKey);
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
