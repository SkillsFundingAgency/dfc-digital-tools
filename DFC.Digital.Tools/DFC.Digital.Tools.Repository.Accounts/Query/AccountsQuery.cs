using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public class AccountsQuery : IAccountQueryRepository
    {
        private readonly DFCUserAccountsContext accountsContext;

        public AccountsQuery(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public IQueryable<Account> GetAccountsThatStillNeedProcessing(DateTime cutOffDate)
        {
            var accounts = from u in accountsContext.Accounts
                           join a in accountsContext.Audit
                           on u.Mail equals a.Email into ua
                           from a in ua.DefaultIfEmpty()
                           where (a == null || a.Status == NotificationProcessingStatus.CircuitGotBroken.ToString()) && u.Createtimestamp < cutOffDate
                           select new Account
                           {
                                Name = u.Name,
                                EMail = u.Mail
                           };

            return accounts;
        }
    }
}
