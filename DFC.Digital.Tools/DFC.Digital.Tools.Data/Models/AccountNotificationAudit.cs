﻿namespace DFC.Digital.Tools.Data.Models
{
    public class AccountNotificationAudit : BaseIntegrationModel
    {
        public string Email { get; set; }

        public NotificationProcessingStatus NotificationProcessingStatus { get; set; }

        public string Note { get; set; }
    }
}
