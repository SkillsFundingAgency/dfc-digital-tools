using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Service.Accounts
{
    public class AccountsService : IAccountsService
    {
        private readonly IApplicationLogger applicationLogger;
        private readonly IConfigConfigurationProvider configuration;
        private readonly IAccountQueryRepository accountQueryRepository;
        private readonly IAuditCommandRepository auditCommandRepository;
        private readonly ICircuitBreakerQueryRepository circuitBreakerQueryRepository;
        private readonly ICircuitBreakerCommandRepository circuitBreakerCommandRepository;

        public AccountsService(
                                IApplicationLogger applicationLogger,
                                IConfigConfigurationProvider configuration,
                                IAccountQueryRepository accountQueryRepository,
                                IAuditCommandRepository auditCommandRepository,
                                ICircuitBreakerQueryRepository circuitBreakerQueryRepository,
                                ICircuitBreakerCommandRepository circuitBreakerCommandRepository)
        {
           this.applicationLogger = applicationLogger;
           this.configuration = configuration;
           this.accountQueryRepository = accountQueryRepository;
           this.auditCommandRepository = auditCommandRepository;
           this.circuitBreakerQueryRepository = circuitBreakerQueryRepository;
           this.circuitBreakerCommandRepository = circuitBreakerCommandRepository;
        }

        public async Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync()
        {
            var circuitBreakerDetails = circuitBreakerQueryRepository.GetBreakerDetails();

            if (circuitBreakerDetails == null)
            {
               applicationLogger.Trace("Adding default circuit breaker record on get as one does not exist");
                return AddDefaultCircuitBreaker();
            }
            else if (circuitBreakerDetails.CircuitBreakerStatus == CircuitBreakerStatus.Open && circuitBreakerDetails.LastCircuitOpenDate.AddHours(24) < DateTime.Now)
            {
                circuitBreakerDetails.CircuitBreakerStatus = CircuitBreakerStatus.Closed;
                circuitBreakerDetails.HalfOpenRetryCount = 0;
                circuitBreakerDetails.LastCircuitOpenDate = DateTime.Now;
                await UpdateCircuitBreakerAsync(circuitBreakerDetails);
            }

            return circuitBreakerDetails;
        }

        public async Task UpdateCircuitBreakerAsync(CircuitBreakerDetails circuitBreakerDetails)
        {
            var updated = circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
            if (!updated)
            {
               applicationLogger.Trace("Adding default circuit breaker record on update as one does not exist");
               AddDefaultCircuitBreaker();
               circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
            }
        }

        public async Task<IEnumerable<Account>> GetNextBatchOfEmailsAsync(int batchSize)
        {
            var nextBatch = accountQueryRepository.GetAccountsThatStillNeedProcessing(this.configuration.GetConfigSectionKey<DateTime>(Constants.AccountRepositorySection, Constants.CutOffDate)).Take(batchSize).ToList();
           applicationLogger.Trace($"Got {nextBatch.Count} records in batch from DB, about to set audit to processing for batch");

           auditCommandRepository.SetBatchToProcessing(nextBatch);
           applicationLogger.Trace($"Set {nextBatch.Count} records for batch to processing");

            return nextBatch;
        }

        public async Task InsertAuditAsync(AccountNotificationAudit accountNotificationAudit)
        {
          auditCommandRepository.Add(accountNotificationAudit);
        }

        public async Task SetBatchToCircuitGotBrokenAsync(IEnumerable<Account> accounts)
        {
          auditCommandRepository.SetBatchToCircuitGotBroken(accounts.ToList());
        }

        public async Task OpenCircuitBreakerAsync()
        {
            await UpdateCircuitBreakerAsync(new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Open,
                LastCircuitOpenDate = DateTime.Now
            });
        }

        public async Task CloseCircuitBreakerAsync()
        {
            await UpdateCircuitBreakerAsync(new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Closed,
                LastCircuitOpenDate = DateTime.Now
            });
        }

        public async Task HalfOpenCircuitBreakerAsync()
        {
            var currentCircuitBreaker = await GetCircuitBreakerStatusAsync();
            if (currentCircuitBreaker?.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen)
            {
                currentCircuitBreaker.HalfOpenRetryCount = currentCircuitBreaker.HalfOpenRetryCount + 1;
               applicationLogger.Trace($"Circuit breaker is half open, setting HalfOpenRetryCount to  {currentCircuitBreaker.HalfOpenRetryCount} ");
            }
            else
            {
               applicationLogger.Trace($"Setting circuit breaker to half open");
                currentCircuitBreaker.CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen;
                currentCircuitBreaker.HalfOpenRetryCount = 0;
                currentCircuitBreaker.LastCircuitOpenDate = DateTime.Now;
            }

            await UpdateCircuitBreakerAsync(currentCircuitBreaker);
        }

        private CircuitBreakerDetails AddDefaultCircuitBreaker()
        {
            var initialCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.Closed, HalfOpenRetryCount = 0, LastCircuitOpenDate = DateTime.Now };
            circuitBreakerCommandRepository.Add(initialCircuitBreaker);
            return initialCircuitBreaker;
        }
    }
}
