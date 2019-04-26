using System;
using System.Linq;
using System.Linq.Expressions;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IQueryRepository<T>
        where T : class
    {
        // Gets entities using delegate
        IQueryable<T> GetMany(Expression<Func<T, bool>> where);
    }
}
