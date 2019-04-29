using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IAuditCommandRepository
    {
        void Add(AccountNotificationAudit entity);

        void SetBatchToProcessing(IList<Account> accounts);
    }
}
