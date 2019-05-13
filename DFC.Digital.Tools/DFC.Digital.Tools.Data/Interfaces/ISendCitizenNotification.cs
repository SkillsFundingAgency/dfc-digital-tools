using DFC.Digital.Tools.Data.Models;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ISendCitizenNotification<in T>
        where T : class
    {
        Task<SendNotificationResponse> SendCitizenNotificationAsync(T notification);
    }
}
