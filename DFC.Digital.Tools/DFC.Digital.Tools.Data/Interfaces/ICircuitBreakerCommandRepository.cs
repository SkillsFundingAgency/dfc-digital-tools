using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ICircuitBreakerCommandRepository
    {
        void Add(CircuitBreakerDetails entity);

        Task<bool> UpdateIfExistsAsync(CircuitBreakerDetails entity);
    }
}
