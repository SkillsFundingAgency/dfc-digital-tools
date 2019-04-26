using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Function.EmailNotification;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.AzureFunctions
{
    public static class ProcessEmailNotificationsFunction
    {
        [FunctionName("ProcessEmailNotificationsFunction")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            Function.Common.ConfigureLog.ConfigureNLogWithAppInsightsTarget();
            log.LogInformation($"{nameof(ProcessEmailNotificationsFunction)} Timer trigger function executed at: {DateTime.Now}");

            await Startup.RunAsync(RunMode.Azure);

            log.LogInformation($"{nameof(ProcessEmailNotificationsFunction)} function completed at: {DateTime.Now}");
        }
    }
}
