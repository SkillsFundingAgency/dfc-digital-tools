using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Repository.Accounts.Command
{
    public class CircuitBreakerCommandRepository : ICircuitBreakerCommandRepository
    {
        private readonly DFCUserAccountsContext accountsContext;

        public CircuitBreakerCommandRepository(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public void Add(CircuitBreakerDetails entity)
        {
            CircuitBreaker circuitBreaker = new CircuitBreaker { CircuitBreakerStatus = entity.CircuitBreakerStatus.ToString(), HalfOpenRetryCount = entity.HalfOpenRetryCount, LastCircuitOpenDate = entity.LastCircuitOpenDate };
            accountsContext.CircuitBreaker.Add(circuitBreaker);
            accountsContext.SaveChanges();
        }

        public async Task<bool> UpdateIfExistsAsync(CircuitBreakerDetails entity)
        {
            var circuitBreaker = accountsContext.CircuitBreaker.FirstOrDefault();
            if (circuitBreaker != null)
            {
                circuitBreaker.CircuitBreakerStatus = entity.CircuitBreakerStatus.ToString();
                circuitBreaker.LastCircuitOpenDate = entity.LastCircuitOpenDate;
                circuitBreaker.HalfOpenRetryCount = entity.HalfOpenRetryCount;
                await accountsContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
