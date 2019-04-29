using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Repository.Accounts.Command
{
    public class AuditCommandRepository : IAuditCommandRepository
    {
        private readonly DFCUserAccountsContext accountsContext;

        public AuditCommandRepository(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public void Add(AccountNotificationAudit accountNotificationAudit)
        {
            Audit audit = new Audit { Email = accountNotificationAudit.Email, Status = accountNotificationAudit.NotificationProcessingStatus.ToString(), Notes = accountNotificationAudit.Note };
            accountsContext.Audit.Add(audit);
            accountsContext.SaveChanges();
        }

        public void SetBatchToProcessing(IList<Account> accounts)
        {
            foreach (var a in accounts)
            {
                Audit audit = new Audit { Email = a.EMail, Status = NotificationProcessingStatus.InProgress.ToString() };
                accountsContext.Audit.Add(audit);
            }

            accountsContext.SaveChanges();
        }
    }
}
