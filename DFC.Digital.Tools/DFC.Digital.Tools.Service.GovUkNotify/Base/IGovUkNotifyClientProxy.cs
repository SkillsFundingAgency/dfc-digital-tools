using Notify.Models.Responses;
using System.Collections.Generic;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    public interface IGovUkNotifyClientProxy
    {
        EmailNotificationResponse SendEmail(string apiKey, string emailAddress, string templateId, Dictionary<string, dynamic> notifyUkDynamicObject);
    }
}