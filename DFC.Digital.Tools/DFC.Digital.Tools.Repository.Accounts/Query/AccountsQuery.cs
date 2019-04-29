using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public class AccountsQuery : IQueryRepository<Account>
    {
        private readonly DFCUserAccountsContext accountsContext;

        public AccountsQuery(DFCUserAccountsContext accountsContext)
        {
            this.accountsContext = accountsContext;
        }

        public IQueryable<Account> GetAll()
        {
            var accounts = from account in accountsContext.Accounts
               select new Account
               {
                 Name = account.Name,
                 EMail = account.Mail
               };

            return accounts;
        }

        public IQueryable<Account> GetMany(Expression<Func<Account, bool>> where)
        {
            return GetAll().Where(where);
        }
    }
}
