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
        /// <summary>
        /// Runs the specified my timer. Every minute every hour
        /// </summary>
        /// <param name="myTimer">My timer.</param>
        /// <param name="log">The log.</param>
        /// <param name="context">Execution context of the funtion</param>
        /// <returns>N/A</returns>
        [FunctionName("ProcessEmailNotificationsFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"{nameof(ProcessEmailNotificationsFunction)} Timer trigger function executed at: {DateTime.Now}");

            await Startup.RunAsync(RunMode.Azure, context.FunctionAppDirectory);

            log.LogInformation($"{nameof(ProcessEmailNotificationsFunction)} function completed at: {DateTime.Now}");
        }
    }
}
