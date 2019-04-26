using System;
using System.Linq.Expressions;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ICommandRepository<T>
        where T : class
    {
        void Add(T entity);
    }
}
