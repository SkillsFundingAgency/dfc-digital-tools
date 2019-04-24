using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Models
{
    public class SemaphoreFlagDetails
    {
        public DateTime LastLockDate { get; set; }

        public bool CircuitClosed { get; set; }
    }
}
