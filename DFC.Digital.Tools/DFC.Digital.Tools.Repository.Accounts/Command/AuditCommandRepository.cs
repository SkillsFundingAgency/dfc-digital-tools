using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Repository.Accounts.Command
{
    public class AuditCommandRepository : IAuditCommandRepository
    {
        private readonly DFCUserAccountsContext accountsContext;

        public AuditCommandRepository(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public async Task AddAsync(AccountNotificationAudit accountNotificationAudit)
        {
            Audit audit = new Audit { Email = accountNotificationAudit.Email, Status = accountNotificationAudit.NotificationProcessingStatus.ToString() };
            if (!string.IsNullOrEmpty(accountNotificationAudit.Note))
            {
                audit.Notes = accountNotificationAudit.Note.Length <= 5000 ? accountNotificationAudit.Note : accountNotificationAudit.Note.Substring(0, 4999);
            }

            accountsContext.Audit.Add(audit);
            await accountsContext.SaveChangesAsync();
        }

        public async Task SetBatchToProcessingAsync(IList<Account> accounts)
        {
            foreach (var a in accounts)
            {
                Audit audit = new Audit { Email = a.EMail, Status = NotificationProcessingStatus.InProgress.ToString() };
                accountsContext.Audit.Add(audit);
            }

            await accountsContext.SaveChangesAsync();
        }

        public async Task SetBatchToCircuitGotBrokenAsync(IList<Account> accounts)
        {
            var audits = accountsContext.Audit.Where(b => accounts.Any(a => a.EMail.Contains(b.Email)) && b.Status == NotificationProcessingStatus.InProgress.ToString()).ToList();
            audits.ForEach(a => a.Status = NotificationProcessingStatus.CircuitGotBroken.ToString());
            await accountsContext.SaveChangesAsync();
        }
    }
}
