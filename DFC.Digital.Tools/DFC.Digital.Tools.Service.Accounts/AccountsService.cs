using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Service.Accounts
{
    public class AccountsService : IAccountsService
    {
        public AccountsService()
        {
        }

        Task<IEnumerable<string>> IAccountsService.GetNextBatchOfEmails(int batchSize)
        {

            throw new NotImplementedException();
        }

        Task IAccountsService.InsertAudit(string email, NotificationProcessingStatus auditStatus, string note)
        {
            throw new NotImplementedException();
        }
    }
}
