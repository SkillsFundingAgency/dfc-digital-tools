using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DFC.Digital.Tools.Repository.Accounts.Query
{
    public class CircuitBreakerQuery : ICircuitBreakerQueryRepository
    {
        private readonly DFCUserAccountsContext accountsContext;

        public CircuitBreakerQuery(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public CircuitBreakerDetails GetBreakerDetails()
        {
            var breaker = (from b in accountsContext.CircuitBreaker
                          select new CircuitBreakerDetails
                          {
                              HalfOpenRetryCount = b.HalfOpenRetryCount,
                              LastCircuitOpenDate = b.LastCircuitOpenDate,
                              CircuitBreakerStatus = (CircuitBreakerStatus)Enum.Parse(typeof(CircuitBreakerStatus), b.CircuitBreakerStatus)
                          }).FirstOrDefault();
            return breaker;
        }
    }
}
