using DFC.Digital.Tools.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public class AccountsQueryRepository : IQueryRepository<Accounts>
    {
        public IQueryable<Accounts> GetMany(Expression<Func<Accounts, bool>> where)
        {
            throw new NotImplementedException();
        }
    }
}
