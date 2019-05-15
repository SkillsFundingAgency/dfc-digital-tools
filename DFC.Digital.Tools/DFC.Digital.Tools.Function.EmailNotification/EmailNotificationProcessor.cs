using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Function.EmailNotification
{
    public class EmailNotificationProcessor : IProcessEmailNotifications
    {
        private readonly ISendCitizenNotification<Account> sendCitizenNotificationService;
        private readonly IApplicationLogger applicationLogger;
        private readonly IConfigConfigurationProvider configuration;
        private readonly IAccountsService accountsService;

        public EmailNotificationProcessor(
            ISendCitizenNotification<Account> sendCitizenNotificationService,
            IApplicationLogger applicationLogger,
            IConfigConfigurationProvider configuration,
            IAccountsService accountsService)
        {
            this.sendCitizenNotificationService = sendCitizenNotificationService;
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
            this.accountsService = accountsService;
        }

        public async Task ProcessEmailNotificationsAsync()
        {
            if (configuration.GetConfig<bool>(Constants.IsDisabled))
            {
                applicationLogger.Trace($"Function is disabled in settings - Not sending any emails.");
            }
            else
            {
                applicationLogger.Trace($"Function is enabled in settings - sending next batch of emails");

                await SendNextBatchOfEmailsAsync();
            }
        }

        private async Task SendNextBatchOfEmailsAsync()
        {
            var circuitBreaker = await accountsService.GetCircuitBreakerStatusAsync();

            if (circuitBreaker.CircuitBreakerStatus != CircuitBreakerStatus.Open)
            {
                var batchSize = configuration.GetConfig<int>(Constants.BatchSize);

                var emailBatch = await accountsService.GetNextBatchOfEmailsAsync(batchSize);

                var accountsToProcess = emailBatch.ToList();
                applicationLogger.Trace($"About to process email notifications with a batch size of {accountsToProcess.Count}");

                var halfOpenCountAllowed = configuration.GetConfig<int>(Constants.GovUkNotifyRetryCount);

                foreach (var account in accountsToProcess)
                {
                    try
                    {
                        var serviceResponse = await sendCitizenNotificationService.SendCitizenNotificationAsync(account);

                        if (serviceResponse.RateLimitException)
                        {
                            await accountsService.OpenCircuitBreakerAsync();
                            applicationLogger.Info(
                                $"RateLimit Exception thrown now resetting the unprocessed email notifications");

                            await accountsService.SetBatchToCircuitGotBrokenAsync(
                                accountsToProcess.Where(notification => !notification.Processed));
                            break;
                        }

                        await accountsService.InsertAuditAsync(new AccountNotificationAudit
                        {
                            Email = account.EMail,
                            NotificationProcessingStatus = serviceResponse.Success
                                ? NotificationProcessingStatus.Completed
                                : NotificationProcessingStatus.Failed
                        });

                        if (serviceResponse.Success &&
                            circuitBreaker.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen)
                        {
                            await accountsService.CloseCircuitBreakerAsync();
                        }

                        account.Processed = true;
                    }
                    catch (Exception exception)
                    {
                        await accountsService.InsertAuditAsync(new AccountNotificationAudit
                        {
                            Email = account.EMail,
                            NotificationProcessingStatus = NotificationProcessingStatus.Failed,
                            Note = exception.InnerException?.Message
                        });

                        await accountsService.HalfOpenCircuitBreakerAsync();
                        applicationLogger.ErrorJustLogIt("Exception whilst sending email notification", exception);
                        circuitBreaker = await accountsService.GetCircuitBreakerStatusAsync();
                        if (circuitBreaker.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen &&
                            circuitBreaker.HalfOpenRetryCount == halfOpenCountAllowed)
                        {
                            await accountsService.OpenCircuitBreakerAsync();

                            //Set the all the accountsin the batch that did not get processed (sent ok)  to CircuitGotBroken
                            await accountsService.SetBatchToCircuitGotBrokenAsync(
                                accountsToProcess.Where(notification => !notification.Processed));
                            break;
                        }
                    }
                }

                applicationLogger.Trace("Completed processing all accounts in the batch");
            }
            else
            {
                applicationLogger.Info("Circuit is open so no account processed");
            }
        }
    }
}
