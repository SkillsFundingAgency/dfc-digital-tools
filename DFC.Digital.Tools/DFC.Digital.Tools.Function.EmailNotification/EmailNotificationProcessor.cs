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
            var emailbatch = accountsService.GetNextBatchOfEmailsAsync(150);

            var circuitBreaker = await circuitBreakerRepository.GetCircuitBreakerStatusAsync();

            circuitBreaker.CircuitBreakerStatus = CircuitBreakerStatus.Open;
            if (circuitBreaker.CircuitBreakerStatus == CircuitBreakerStatus.Closed)
            {
                var emailsToProcess = await citizenEmailRepository.GetCitizenEmailNotificationsAsync();
                applicationLogger.Trace($"About to process email notifications with a batch size of {emailsToProcess.Count()}");

                var halfOpenCountAllowed = configuration.GetConfigSectionKey<int>(Constants.GovUkNotifySection, Constants.GovUkNotifyRetryCount);

                foreach (var email in emailsToProcess)
                {
                    try
                    {
                        var sent = await sendCitizenNotificationService.SendCitizenNotificationAsync(email);

                        email.NotificationProcessingStatus = sent ? NotificationProcessingStatus.Completed : NotificationProcessingStatus.Failed;
                        await citizenEmailRepository.UpdateCitizenEmailNotificationAsync(email);
                    }
                    catch (RateLimitException exception)
                    {
                            applicationLogger.Info($"RateLimit Exception  now resetting the unprocessed email notifications :- {exception.Message}");
                            await citizenEmailRepository.ResetCitizenEmailNotificationAsync(
                                emailsToProcess.Where(notification =>
                                    notification.NotificationProcessingStatus ==
                                    NotificationProcessingStatus.InProgress));
                            break;
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
