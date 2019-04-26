using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using Notify.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    public class GovUkNotifyService : ISendCitizenNotification<CitizenEmailNotification>
    {
        private readonly IApplicationLogger applicationLogger;
        private readonly IGovUkNotifyClientProxy clientProxy;
        private readonly IConfigConfigurationProvider configuration;
        private readonly ICircuitBreakerRepository circuitBreakerRepository;

        public GovUkNotifyService(IApplicationLogger applicationLogger, IGovUkNotifyClientProxy clientProxy, IConfigConfigurationProvider configuration, ICircuitBreakerRepository circuitBreakerRepository)
        {
            this.applicationLogger = applicationLogger;
            this.clientProxy = clientProxy;
            this.configuration = configuration;
            this.circuitBreakerRepository = circuitBreakerRepository;
        }

        public async Task<bool> SendCitizenNotificationAsync(CitizenEmailNotification notification)
        {
            try
            {
                var response = clientProxy.SendEmail(
                    configuration.GetConfigSectionKey<string>(Constants.GovUkNotifySection, Constants.GovUkNotifyApiKey),
                    notification.EmailAddress,
                    configuration.GetConfigSectionKey<string>(Constants.GovUkNotifySection, Constants.GovUkNotifyTemplateId),
                    this.Convert(notification.EmailPersonalisation));
                return !string.IsNullOrEmpty(response?.id);
            }
            catch (NotifyClientException ex)
            {
                applicationLogger.Error("Failed to send citizen email with GovUKNotify", ex);
                if (ex.Message.ToLowerInvariant().Contains(configuration.GetConfigSectionKey<string>(Constants.GovUkNotifySection, Constants.GovUkNotifyRateLimitException)))
                {
                     await circuitBreakerRepository.OpenCircuitBreakerAsync();
                    throw new RateLimitException();
                }

               return false;
            }
        }

        public Dictionary<string, dynamic> Convert(GovUkNotifyPersonalisation govUkNotifyPersonalisation)
        {
            if (govUkNotifyPersonalisation?.Personalisation != null)
            {
                foreach (var item in govUkNotifyPersonalisation.Personalisation?.ToArray())
                {
                    if (string.IsNullOrEmpty(item.Value))
                    {
                        govUkNotifyPersonalisation.Personalisation[item.Key] = Constants.UnknownValue;
                    }
                }

                return govUkNotifyPersonalisation?.Personalisation
                    .ToDictionary<KeyValuePair<string, string>, string, dynamic>(
                        vocObj => vocObj.Key,
                        vocObj => vocObj.Value);
            }

            return null;
        }
    }
}
