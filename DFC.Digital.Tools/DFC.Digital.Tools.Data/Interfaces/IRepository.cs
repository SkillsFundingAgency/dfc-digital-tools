namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IRepository<T> : IQueryRepository<T>, ICommandRepository<T>
        where T : class
    {
    }
}
