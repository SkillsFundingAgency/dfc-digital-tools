using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IAccountsService
    {
        Task<IEnumerable<Account>> GetNextBatchOfEmailsAsync(int batchSize);

        Task InsertAuditAsync(AccountNotificationAudit accountNotificationAudit);

        Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync();
    }
}
