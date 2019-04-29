using DFC.Digital.Tools.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DFC.Digital.Tools.Repository.Accounts.Query
{
    public class CircuitBreakerQuery : IQueryRepository<CircuitBreaker>
    {
        public IQueryable<CircuitBreaker> GetMany(Expression<Func<CircuitBreaker, bool>> where)
        {
            throw new NotImplementedException();
        }

        IQueryable<CircuitBreaker> IQueryRepository<CircuitBreaker>.GetAll()
        {
            throw new NotImplementedException();
        }

        IQueryable<CircuitBreaker> IQueryRepository<CircuitBreaker>.GetMany(Expression<Func<CircuitBreaker, bool>> where)
        {
            throw new NotImplementedException();
        }
    }
}
