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

        public AccountsService(
                                IApplicationLogger applicationLogger,
                                IConfigConfigurationProvider configuration,
                                IAccountQueryRepository accountQueryRepository,
                                IAuditCommandRepository auditCommandRepository,
                                ICircuitBreakerQueryRepository circuitBreakerQueryRepository)
        {
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
            this.accountQueryRepository = accountQueryRepository;
            this.auditCommandRepository = auditCommandRepository;
            this.circuitBreakerQueryRepository = circuitBreakerQueryRepository;
        }

        public async Task<CircuitBreakerDetails> GetCircuitBreakerStatusAsync()
        {
            return this.circuitBreakerQueryRepository.GetBreakerDetails();
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
