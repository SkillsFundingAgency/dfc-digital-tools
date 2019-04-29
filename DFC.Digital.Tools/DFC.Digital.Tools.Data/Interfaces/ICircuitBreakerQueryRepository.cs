using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ICircuitBreakerQueryRepository
    {
        CircuitBreakerDetails GetBreakerDetails();
    }
}
