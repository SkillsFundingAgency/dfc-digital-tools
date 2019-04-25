using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using Notify.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    public class GovUkNotifyService : ISendCitizenNotification<CitizenEmailNotification>
    {
        private readonly IApplicationLogger applicationLogger;
        private readonly IGovUkNotifyClientProxy clientProxy;
        private readonly IConfigConfigurationProvider configuration;

        public GovUkNotifyService(IApplicationLogger applicationLogger, IGovUkNotifyClientProxy clientProxy, IConfigConfigurationProvider configuration)
        {
            this.applicationLogger = applicationLogger;
            this.clientProxy = clientProxy;
            this.configuration = configuration;
        }

        public bool SendCitizenNotification(CitizenEmailNotification notification)
        {
            try
            {
                var response = clientProxy.SendEmail(configuration.GetConfig<string>(Constants.GovUkNotifyApiKey), notification.EmailAddress, configuration.GetConfig<string>(Constants.GovUkNotifyTemplateId), this.Convert(notification.EmailPersonalisation));
                return !string.IsNullOrEmpty(response?.id);
            }
            catch (NotifyClientException ex)
            {
                applicationLogger.Error("Failed to send VOC email", ex);
                return false;
            }
        }

        public Dictionary<string, dynamic> Convert(GovUkNotifyPersonalisation govUkNotifyPersonalisation)
        {
            if (govUkNotifyPersonalisation?.Personalisation != null)
            {
                foreach (var item in govUkNotifyPersonalisation?.Personalisation?.ToArray())
                {
                    if (string.IsNullOrEmpty(item.Value) && govUkNotifyPersonalisation != null)
                    {
                        govUkNotifyPersonalisation.Personalisation[item.Key] = "uknown";
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
