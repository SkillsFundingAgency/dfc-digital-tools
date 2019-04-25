using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Repository.Pirean
{
    public class PireanRepository : ICitizenNotificationRepository<CitizenEmailNotification>, ISemaphoreFlagDetailsRepository
    {
        public async Task<IQueryable<CitizenEmailNotification>> GetCitizenEmailNotificationsAsync()
        {
          var result = await Task.Run(GetList);
          return result.AsQueryable();
        }

        public Task UpdateCitizenEmailNotificationAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<SemaphoreFlagDetails> GetSemaphoreFlagDetailsAsync()
        {
            return await Task.Run(GetSemaphoreFlagDetails);
        }

        public Task UpdateSemaphoreFlagDetailsAsync()
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<CitizenEmailNotification> GetList()
        {
            yield return new CitizenEmailNotification { EmailAddress = nameof(CitizenEmailNotification.EmailAddress), EmailPersonalisation = GetGovUkNotifyPersonalisation() };
            yield return new CitizenEmailNotification { EmailAddress = nameof(CitizenEmailNotification.EmailAddress), EmailPersonalisation = GetGovUkNotifyPersonalisation() };
        }

        private SemaphoreFlagDetails GetSemaphoreFlagDetails()
        {
            return new SemaphoreFlagDetails { CircuitClosed = true };
        }

        private GovUkNotifyPersonalisation GetGovUkNotifyPersonalisation()
        {
            var citizenDetails = new GovUkNotifyPersonalisation();
            citizenDetails.Personalisation.Add(nameof(CitizenEmailNotification.Firstname), nameof(CitizenEmailNotification.Firstname));
            citizenDetails.Personalisation.Add(nameof(CitizenEmailNotification.Lastname), nameof(CitizenEmailNotification.Lastname));
            return citizenDetails;
        }
    }
}
