using DFC.Digital.Tools.Data.Models;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ICircuitBreakerRepository
    {
        Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync();

        Task OpenCircuitBreakerAsync();

        Task HalfOpenCircuitBreakerAsync();
    }
}
