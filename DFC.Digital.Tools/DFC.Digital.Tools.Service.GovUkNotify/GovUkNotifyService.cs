using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using Notify.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    public class GovUkNotifyService : ISendCitizenNotification<Account>
    {
        private readonly IApplicationLogger applicationLogger;
        private readonly IGovUkNotifyClientProxy clientProxy;
        private readonly IConfigConfigurationProvider configuration;
        private readonly IAccountsService accountsService;

        public GovUkNotifyService(IApplicationLogger applicationLogger, IGovUkNotifyClientProxy clientProxy, IConfigConfigurationProvider configuration, IAccountsService accountsService)
        {
            this.applicationLogger = applicationLogger;
            this.clientProxy = clientProxy;
            this.configuration = configuration;
            this.accountsService = accountsService;
        }

        public async Task<SendNotificationResponse> SendCitizenNotificationAsync(Account notification)
        {
            var sendNotificationResponse = new SendNotificationResponse();
            try
            {
                var response = clientProxy.SendEmail(
                    configuration.GetConfigSectionKey<string>(
                        Constants.GovUkNotifySection,
                        Constants.GovUkNotifyApiKey),
                    notification.EMail,
                    configuration.GetConfigSectionKey<string>(
                        Constants.GovUkNotifySection,
                        Constants.GovUkNotifyTemplateId),
                    Convert(GetGovUkNotifyPersonalisation(notification)));
                sendNotificationResponse.Success = !string.IsNullOrEmpty(response?.id);
            }
            catch (NotifyClientException ex)
            {
                if (ex.Message.ToLowerInvariant()
                    .Contains(configuration.GetConfigSectionKey<string>(
                        Constants.GovUkNotifySection,
                        Constants.GovUkNotifyRateLimitException)))
                {
                    sendNotificationResponse.RateLimitException = true;
                    await accountsService.OpenCircuitBreakerAsync();
                }

                applicationLogger.Error("Failed to send citizen email with GovUKNotify", ex);
            }

            return sendNotificationResponse;
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

        private GovUkNotifyPersonalisation GetGovUkNotifyPersonalisation(Account account)
        {
           var result = new GovUkNotifyPersonalisation();
           result.Personalisation.Add(nameof(Account.Name), account.Name);
           return result;
        }
    }
}
