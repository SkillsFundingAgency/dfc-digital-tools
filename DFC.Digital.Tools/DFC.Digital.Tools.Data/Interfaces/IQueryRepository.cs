using System;
using System.Linq;
using System.Linq.Expressions;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IQueryRepository<T>
        where T : class
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Default repository pattern, changing it doesnt make sense.")]
        IQueryable<T> GetAll();

        // Gets entities using delegate
        IQueryable<T> GetMany(Expression<Func<T, bool>> where);
    }
}
