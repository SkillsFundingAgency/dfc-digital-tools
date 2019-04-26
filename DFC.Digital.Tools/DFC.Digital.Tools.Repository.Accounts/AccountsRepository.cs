using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Repository.Pirean
{
    public class AccountsRepository : ICitizenNotificationRepository<CitizenEmailNotification>, ICircuitBreakerRepository
    {
        public async Task<IQueryable<CitizenEmailNotification>> GetCitizenEmailNotificationsAsync()
        {
            //throw new NotImplementedException();
            var result = await Task.Run(GetList);
            return result.AsQueryable();
        }

        public async Task UpdateCitizenEmailNotificationAsync(CitizenEmailNotification emailNotification)
        {
            await Task.Run(() => Task.Delay(1));
        }

        public async Task ResetCitizenEmailNotificationAsync(IQueryable<CitizenEmailNotification> emailNotification)
        {
            await Task.Run(() => Task.Delay(1));
        }

        public async Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync()
        {
            //throw new NotImplementedException();
            return await Task.Run(GetCircuitBreakerDetails);
        }

        public async Task OpenCircuitBreakerAsync()
        {
            await Task.Run(() => Task.Delay(1));
        }

        public async Task HalfOpenCircuitBreakerAsync()
        {
            await Task.Run(() => Task.Delay(1));
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
            yield return new CitizenEmailNotification { EmailAddress = "trevk15@yahoo.co.uk", EmailPersonalisation = GetGovUkNotifyPersonalisation() };
            yield return new CitizenEmailNotification { EmailAddress = "trevk155@gmail.com", EmailPersonalisation = GetGovUkNotifyPersonalisation() };
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
