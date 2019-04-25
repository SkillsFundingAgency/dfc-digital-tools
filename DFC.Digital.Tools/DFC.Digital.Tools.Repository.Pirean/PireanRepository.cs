using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;

namespace DFC.Digital.Tools.Repository.Pirean
{
    public class PireanRepository : ICitizenNotificationRepository<CitizenEmailNotification>, ICircuitBreakerRepository
    {
        public async Task<IQueryable<CitizenEmailNotification>> GetCitizenEmailNotificationsAsync()
        {
          var result = await Task.Run(GetList);
          return result.AsQueryable();
        }

        public Task UpdateCitizenEmailNotificationAsync(CitizenEmailNotification emailNotification)
        {
            throw new NotImplementedException();
        }

        public Task ResetCitizenEmailNotificationAsync(IQueryable<CitizenEmailNotification> emailNotification)
        {
            throw new NotImplementedException();
        }

        public async Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync()
        {
            return await Task.Run(GetCircuitBreakerDetails);
        }

        public Task OpenCircuitBreakerAsync()
        {
            throw new NotImplementedException();
        }

        public Task HalfOpenCircuitBreakerAsync()
        {
            throw new NotImplementedException();
        }

        private static CircuitBreakerDetails GetCircuitBreakerDetails()
        {
            var circuitBreaker = new CircuitBreakerDetails();

            if (circuitBreaker.LastCircuitOpenDate > DateTime.MinValue)
            {
                circuitBreaker.CircuitBreakerStatus = circuitBreaker.LastCircuitOpenDate.AddHours(24) > DateTime.Now
                    ? CircuitBreakerStatus.Open
                    : CircuitBreakerStatus.Closed;
            }

            return circuitBreaker;
        }

        private IEnumerable<CitizenEmailNotification> GetList()
        {
            yield return new CitizenEmailNotification { EmailAddress = nameof(CitizenEmailNotification.EmailAddress), EmailPersonalisation = GetGovUkNotifyPersonalisation() };
            yield return new CitizenEmailNotification { EmailAddress = nameof(CitizenEmailNotification.EmailAddress), EmailPersonalisation = GetGovUkNotifyPersonalisation() };
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
