using DFC.Digital.Tools.Data.Models;
using DFC.Digital.Tools.Repository.Accounts.Command;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Testing.FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Digital.Tools.Repository.Accounts.UnitTests
{
    public class AuditCommandRepositoryTests
    {
        private readonly DFCUserAccountsContext fakeDbContext;
        private readonly AuditCommandRepository repo;
        private readonly List<Audit> testAuditTable;

        public AuditCommandRepositoryTests()
        {
           fakeDbContext = A.Fake<DFCUserAccountsContext>();
           repo = new AuditCommandRepository(this.fakeDbContext);
           testAuditTable = new List<Audit>();
            var fakeAuditDbSet = Aef.FakeDbSet(this.testAuditTable);
            A.CallTo(() => fakeDbContext.Audit).Returns(fakeAuditDbSet);
        }

        [Fact]
        public void AddAuditTest()
        {
            // Act
           repo.AddAsync(new AccountNotificationAudit() { Email = nameof(AccountNotificationAudit.Email), NotificationProcessingStatus = NotificationProcessingStatus.Completed, Note = nameof(AccountNotificationAudit.Note) }).Wait();

            // Asserts
            A.CallTo(() => fakeDbContext.Audit.Add(A<Audit>._)).MustHaveHappened();

           testAuditTable.Count().Should().Be(1);

            var insertedAudit = testAuditTable.FirstOrDefault();
            insertedAudit.Email.Should().Be(nameof(AccountNotificationAudit.Email));
            insertedAudit.Status.Should().Be(NotificationProcessingStatus.Completed.ToString());
            insertedAudit.Notes.Should().Be(nameof(AccountNotificationAudit.Note));
        }

        [Fact]
        public void SetAuditBatchToProcessingTest()
        {
            // Setup
            var accounts = SetUpTestAccounts();

            // Act
            repo.SetBatchToProcessingAsync(accounts).Wait();

            // Asserts
            A.CallTo(() => fakeDbContext.Audit.Add(A<Audit>._)).MustHaveHappenedTwiceExactly();
           CheckExpectedResults(accounts, NotificationProcessingStatus.InProgress.ToString());
        }

        [Fact]
        public void SetAuditBatchToCircuitGotBrokenTest()
        {
            // Setup
            var accounts = SetUpTestAccounts();

            // Act
           repo.SetBatchToProcessingAsync(accounts).Wait();
           repo.SetBatchToCircuitGotBrokenAsync(accounts).Wait();

            // Asserts
           CheckExpectedResults(accounts, NotificationProcessingStatus.CircuitGotBroken.ToString());
        }

        private void CheckExpectedResults(List<Account> accounts, string expectedStatus)
        {
           testAuditTable.Count().Should().Be(accounts.Count);

            var insertedAudit1 = testAuditTable.FirstOrDefault();
            var account1 = accounts.FirstOrDefault();
            insertedAudit1.Email.Should().Be(account1.EMail);
            insertedAudit1.Status.Should().Be(expectedStatus);

            var insertedAudit2 = testAuditTable.TakeLast(1).FirstOrDefault();
            var account2 = accounts.TakeLast(1).FirstOrDefault();
            insertedAudit2.Email.Should().Be(account2.EMail);
            insertedAudit2.Status.Should().Be(expectedStatus);
        }

        private List<Account> SetUpTestAccounts()
        {
            List<Account> accounts = new List<Account>
            {
              new Account { EMail = "test_Email_1" },
              new Account { EMail = "test_Email_2" }
            };
            return accounts;
        }
    }
}