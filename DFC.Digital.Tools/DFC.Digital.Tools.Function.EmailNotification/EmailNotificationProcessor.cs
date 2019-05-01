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
        private readonly ISendCitizenNotification<CitizenEmailNotification> sendCitizenNotificationService;
        private readonly ICitizenNotificationRepository<CitizenEmailNotification> citizenEmailRepository;
        private readonly IApplicationLogger applicationLogger;
        private readonly ICircuitBreakerRepository circuitBreakerRepository;
        private readonly IConfigConfigurationProvider configuration;
        private readonly IAccountsService accountsService;

        public EmailNotificationProcessor(
            ISendCitizenNotification<CitizenEmailNotification> sendCitizenNotificationService,
            ICitizenNotificationRepository<CitizenEmailNotification> citizenEmailRepository,
            IApplicationLogger applicationLogger,
            ICircuitBreakerRepository circuitBreakerRepository,
            IConfigConfigurationProvider configuration,
            IAccountsService accountsService)
        {
            this.citizenEmailRepository = citizenEmailRepository;
            this.sendCitizenNotificationService = sendCitizenNotificationService;
            this.applicationLogger = applicationLogger;
            this.circuitBreakerRepository = circuitBreakerRepository;
            this.configuration = configuration;
            this.accountsService = accountsService;
        }

        public async Task ProcessEmailNotificationsAsync()
        {
            var circuitBreaker = await circuitBreakerRepository.GetCircuitBreakerStatusAsync();

            if (circuitBreaker.CircuitBreakerStatus != CircuitBreakerStatus.Open)
            {
               // var emailbatch = await accountsService.GetNextBatchOfEmailsAsync(configuration.GetConfigSectionKey<int>(Constants.AccountRepositorySection, Constants.BatchSize));
               //await accountsService.SetBatchToCircuitGotBrokenAsync(emailbatch);
                var emailsToProcess = await citizenEmailRepository.GetCitizenEmailNotificationsAsync();
                applicationLogger.Trace($"About to process email notifications with a batch size of {emailsToProcess.Count()}");

                var halfOpenCountAllowed = configuration.GetConfigSectionKey<int>(Constants.GovUkNotifySection, Constants.GovUkNotifyRetryCount);

                foreach (var email in emailsToProcess)
                {
                    try
                    {
                        var serviceResponse = await sendCitizenNotificationService.SendCitizenNotificationAsync(email);

                        if (serviceResponse.RateLimitException)
                        {
                            await circuitBreakerRepository.OpenCircuitBreakerAsync();
                            applicationLogger.Info(
                                $"RateLimit Exception thrown now resetting the unprocessed email notifications");
                            await citizenEmailRepository.ResetCitizenEmailNotificationAsync(
                                emailsToProcess.Where(notification =>
                                    notification.NotificationProcessingStatus ==
                                    NotificationProcessingStatus.InProgress));
                            break;
                        }
                        else
                        {
                            email.NotificationProcessingStatus = serviceResponse.Success
                                ? NotificationProcessingStatus.Completed
                                : NotificationProcessingStatus.Failed;
                            await citizenEmailRepository.UpdateCitizenEmailNotificationAsync(email);
                        }
                    }
                    catch (Exception exception)
                    {
                        await circuitBreakerRepository.HalfOpenCircuitBreakerAsync();
                        applicationLogger.Error("Exception whilst sending email notification", exception);
                        circuitBreaker = await circuitBreakerRepository.GetCircuitBreakerStatusAsync();
                        if (circuitBreaker.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen &&
                            circuitBreaker.HalfOpenRetryCount == halfOpenCountAllowed)
                        {
                            await circuitBreakerRepository.OpenCircuitBreakerAsync();
                            await citizenEmailRepository.ResetCitizenEmailNotificationAsync(
                                emailsToProcess.Where(notification =>
                                    notification.NotificationProcessingStatus ==
                                    NotificationProcessingStatus.InProgress));
                            break;
                        }
                    }
                }

                applicationLogger.Trace("Completed processing all email notifications from the recycle bin");
            }
        }
    }
}
