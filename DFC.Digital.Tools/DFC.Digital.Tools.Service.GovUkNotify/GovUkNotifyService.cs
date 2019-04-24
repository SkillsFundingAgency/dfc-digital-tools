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

        public GovUkNotifyService(IApplicationLogger applicationLogger, IGovUkNotifyClientProxy clientProxy)
        {
            this.applicationLogger = applicationLogger;
            this.clientProxy = clientProxy;
        }

        public bool SendCitizenNotification(CitizenEmailNotification notification)
        {
            try
            {
                var response = clientProxy.SendEmail("apikey", notification.EmailAddress, "templateId", this.Convert(notification.EmailPersonalisation));
                return !string.IsNullOrEmpty(response?.id);
            }
            catch (NotifyClientException ex)
            {
                applicationLogger.ErrorJustLogIt("Failed to send VOC email", ex);
                return false;
            }
        }

        public Dictionary<string, dynamic> Convert(GovUkNotifyPersonalisation vocSurveyPersonalisation)
        {
            if (vocSurveyPersonalisation?.Personalisation != null)
            {
                foreach (var item in vocSurveyPersonalisation?.Personalisation?.ToArray())
                {
                    if (string.IsNullOrEmpty(item.Value) && vocSurveyPersonalisation != null)
                    {
                        vocSurveyPersonalisation.Personalisation[item.Key] = "uknown";
                    }
                }

                return vocSurveyPersonalisation?.Personalisation
                    .ToDictionary<KeyValuePair<string, string>, string, dynamic>(
                        vocObj => vocObj.Key,
                        vocObj => vocObj.Value);
            }

            return null;
        }
    }
}
