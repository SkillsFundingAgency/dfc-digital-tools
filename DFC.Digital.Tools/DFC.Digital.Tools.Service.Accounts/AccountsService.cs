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
                 return this.AddDefaultCircuitBreaker();
            }

            return circuitBreakerDetails;
        }

        public async Task UpdateCircuitBreakerAsync(CircuitBreakerDetails circuitBreakerDetails)
        {
            var updated = this.circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
            if (!updated)
            {
                this.AddDefaultCircuitBreaker();
            }

            this.circuitBreakerCommandRepository.UpdateIfExists(circuitBreakerDetails);
        }

        private CircuitBreakerDetails AddDefaultCircuitBreaker()
        {
            var initialCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.Closed, HalfOpenRetryCount = 0, LastCircuitOpenDate = DateTime.Now };
            this.circuitBreakerCommandRepository.Add(initialCircuitBreaker);
            return initialCircuitBreaker;
        }

        public async Task<IEnumerable<Account>> GetNextBatchOfEmailsAsync(int batchSize)
        {
            var nextBatch = this.accountQueryRepository.GetAccountsThatStillNeedProcessing().Take(batchSize).ToList();
            this.auditCommandRepository.SetBatchToProcessing(nextBatch);
            return nextBatch;
        }

        public async Task InsertAuditAsync(AccountNotificationAudit accountNotificationAudit)
        {
           this.auditCommandRepository.Add(accountNotificationAudit);
        }
    }
}
