using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Models
{
    public class SendNotificationResponse
    {
        public bool Success { get; set; }

        public bool RateLimitException { get; set; }
    }
}
