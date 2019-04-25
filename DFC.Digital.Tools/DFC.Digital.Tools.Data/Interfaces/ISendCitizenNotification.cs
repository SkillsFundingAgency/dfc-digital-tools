using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ISendCitizenNotification<in T>
        where T : CitizenNotification
    {
        Task<bool> SendCitizenNotificationAsync(T notification);
    }
}
