using System;
using System.Collections.Generic;

namespace DFC.Digital.Tools.Repository.Accounts.DataModel
{
    public partial class CircuitBreaker
    {
        public int Id { get; set; }

        public DateTime? LastCircuitOpenDate { get; set; }

        public string CircuitBreakerStatus { get; set; }

        public int? HalfOpenRetryCount { get; set; }
    }
}
