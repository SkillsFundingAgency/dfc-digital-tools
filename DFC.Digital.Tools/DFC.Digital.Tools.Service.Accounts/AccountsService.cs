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
            var circuitBreakerDetails = this.circuitBreakerQueryRepository.GetBreakerDetails();

            if (circuitBreakerDetails == null)
            {
                this.applicationLogger.Trace("Adding default circuit breaker record on get as one does not exist");
                return this.AddDefaultCircuitBreaker();
            }
            else if (circuitBreakerDetails.CircuitBreakerStatus == CircuitBreakerStatus.Open && circuitBreakerDetails.LastCircuitOpenDate.AddHours(24) < DateTime.Now)
            {
                circuitBreakerDetails.CircuitBreakerStatus = CircuitBreakerStatus.Closed;
                circuitBreakerDetails.HalfOpenRetryCount = 0;
                circuitBreakerDetails.LastCircuitOpenDate = DateTime.Now;
                await this.UpdateCircuitBreakerAsync(circuitBreakerDetails);
            }

            return circuitBreakerDetails;
        }

        public async Task UpdateCircuitBreakerAsync(CircuitBreakerDetails circuitBreakerDetails)
        {
            var updated = this.circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
            if (!updated)
            {
                this.applicationLogger.Trace("Adding default circuit breaker record on update as one does not exist");
                this.AddDefaultCircuitBreaker();
                this.circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
            }
        }

        public async Task<IEnumerable<Account>> GetNextBatchOfEmailsAsync(int batchSize)
        {
            var nextBatch = this.accountQueryRepository.GetAccountsThatStillNeedProcessing(this.configuration.GetConfigSectionKey<DateTime>(Constants.AccountRepositorySection, Constants.CutOffDate)).Take(batchSize).ToList();
            this.applicationLogger.Trace($"Got {nextBatch.Count} records in batch from DB, about to set audit to processing for batch");

            this.auditCommandRepository.SetBatchToProcessing(nextBatch);
            this.applicationLogger.Trace($"Set {nextBatch.Count} records for batch to processing");

            return nextBatch;
        }

        public async Task InsertAuditAsync(AccountNotificationAudit accountNotificationAudit)
        {
           this.auditCommandRepository.Add(accountNotificationAudit);
        }

        public async Task SetBatchToCircuitGotBrokenAsync(IEnumerable<Account> accounts)
        {
           this.auditCommandRepository.SetBatchToCircuitGotBroken(accounts.ToList());
        }

        public async Task OpenCircuitBreakerAsync()
        {
            await this.UpdateCircuitBreakerAsync(new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Open,
                LastCircuitOpenDate = DateTime.Now
            });
        }

        public async Task CloseCircuitBreakerAsync()
        {
            await this.UpdateCircuitBreakerAsync(new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Closed,
                LastCircuitOpenDate = DateTime.Now
            });
        }

        public async Task HalfOpenCircuitBreakerAsync()
        {
            var currentCircuitBreaker = await this.GetCircuitBreakerStatusAsync();
            if (currentCircuitBreaker?.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen)
            {
                currentCircuitBreaker.HalfOpenRetryCount = currentCircuitBreaker.HalfOpenRetryCount + 1;
                this.applicationLogger.Trace($"Circuit breaker is half open, setting HalfOpenRetryCount to  {currentCircuitBreaker.HalfOpenRetryCount} ");
                await this.UpdateCircuitBreakerAsync(currentCircuitBreaker);
            }
            else
            {
                this.applicationLogger.Trace($"Setting circuit breaker to half open");
                await this.UpdateCircuitBreakerAsync(new CircuitBreakerDetails
                {
                    CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen,
                    LastCircuitOpenDate = DateTime.Now
                });
            }
        }

        private CircuitBreakerDetails AddDefaultCircuitBreaker()
        {
            var initialCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.Closed, HalfOpenRetryCount = 0, LastCircuitOpenDate = DateTime.Now };
            this.circuitBreakerCommandRepository.Add(initialCircuitBreaker);
            return initialCircuitBreaker;
        }
    }
}
