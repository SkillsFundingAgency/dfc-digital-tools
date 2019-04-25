using DFC.Digital.Tools.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface ICitizenNotificationRepository<T>
    where T : class
    {
       Task<IQueryable<T>> GetCitizenEmailNotificationsAsync();

       Task UpdateCitizenEmailNotificationAsync(T emailNotification);

       Task ResetCitizenEmailNotificationAsync(IQueryable<T> emailNotification);
    }
}
