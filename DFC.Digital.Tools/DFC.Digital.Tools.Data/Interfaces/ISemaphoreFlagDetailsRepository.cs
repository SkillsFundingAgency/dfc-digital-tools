using DFC.Digital.Tools.Data.Models;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ISemaphoreFlagDetailsRepository
    {
        Task<SemaphoreFlagDetails> GetSemaphoreFlagDetailsAsync();

        Task UpdateSemaphoreFlagDetailsAsync();
    }
}
