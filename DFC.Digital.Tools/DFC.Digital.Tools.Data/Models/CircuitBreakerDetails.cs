using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Models
{
    public class CircuitBreakerDetails
    {
        public DateTime LastCircuitOpenDate { get; set; }

        public CircuitBreakerStatus CircuitBreakerStatus { get; set; }

        public int HalfOpenRetryCount { get; set; }
    }
}
