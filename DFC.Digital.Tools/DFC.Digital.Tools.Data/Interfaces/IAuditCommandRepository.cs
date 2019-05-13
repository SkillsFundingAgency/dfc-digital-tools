using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IAuditCommandRepository
    {
        Task AddAsync(AccountNotificationAudit entity);

        Task SetBatchToProcessingAsync(IList<Account> accounts);

        Task SetBatchToCircuitGotBrokenAsync(IList<Account> accounts);
    }
}
