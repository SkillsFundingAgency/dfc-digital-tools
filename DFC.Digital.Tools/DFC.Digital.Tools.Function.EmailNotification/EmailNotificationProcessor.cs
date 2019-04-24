using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Function.EmailNotification
{
    public class EmailNotificationProcessor
    {
        private readonly ISendCitizenNotification<CitizenEmailNotification> sendCitizenNotificationService;
        private readonly ICitizenNotificationRepository<CitizenEmailNotification> citizenEmailRepository;
        private readonly IApplicationLogger applicationLogger;
        private readonly ISemaphoreFlagDetailsRepository semaphoreFlagDetailsRepository;

        public EmailNotificationProcessor(
            ISendCitizenNotification<CitizenEmailNotification> sendCitizenNotificationService,
            ICitizenNotificationRepository<CitizenEmailNotification> citizenEmailRepository,
          IApplicationLogger applicationLogger,
            ISemaphoreFlagDetailsRepository semaphoreFlagDetailsRepository)
        {
            this.citizenEmailRepository = citizenEmailRepository;
            this.sendCitizenNotificationService = sendCitizenNotificationService;
            this.applicationLogger = applicationLogger;
            this.semaphoreFlagDetailsRepository = semaphoreFlagDetailsRepository;
        }

        public async Task ProcessEmailNotifications()
        {
            var semaphoreFlag = await semaphoreFlagDetailsRepository.GetSemaphoreFlagDetailsAsync();

            if (semaphoreFlag.CircuitClosed)
            {
                var emailsToProcess = await citizenEmailRepository.GetCitizenEmailNotificationsAsync();
                applicationLogger.Trace($"About to process email notifications with a batch size of {emailsToProcess.Count()}");

                foreach (var email in emailsToProcess)
                {
                    try
                    {
                        var sent = sendCitizenNotificationService.SendCitizenNotification(email);
                    }
                    catch (Exception exception)
                    {
                        if (exception.Message.Contains("429"))
                        {
                            applicationLogger.Error("Exception containing a status code of 429 whilst sending email notification", exception);
                            await semaphoreFlagDetailsRepository.UpdateSemaphoreFlagDetailsAsync();
                            break;
                        }

                        applicationLogger.Error("Exception whilst sending email notification", exception);
                    }
                }

                applicationLogger.Trace("Completed processing all email notifications from the recycle bin");
            }
        }
    }
}
