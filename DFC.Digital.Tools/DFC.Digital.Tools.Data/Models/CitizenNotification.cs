﻿using DFC.Digital.Tools.Data.Models;

namespace DFC.Digital.Tools.Data
{
    public class CitizenNotification : BaseIntegrationModel
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string LifeLongLearningAccountId { get; set; }

        public NotificationProcessingStatus NotificationProcessingStatus { get; set; }
    }
}
