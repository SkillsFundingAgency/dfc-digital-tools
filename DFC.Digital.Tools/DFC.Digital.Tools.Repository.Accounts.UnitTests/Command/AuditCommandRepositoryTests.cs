namespace DFC.Digital.Tools.Repository.Accounts.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DFC.Digital.Tools.Data.Models;
    using DFC.Digital.Tools.Repository.Accounts.Command;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Testing.FakeItEasy;
    using Xunit;

    public class AuditCommandRepositoryTests
    {
        private readonly DFCUserAccountsContext fakeDbContext;
        private readonly AuditCommandRepository repo;
        private readonly List<Audit> testAuditTable;

        public AuditCommandRepositoryTests()
        {
            this.fakeDbContext = A.Fake<DFCUserAccountsContext>();
            this.repo = new AuditCommandRepository(this.fakeDbContext);
            this.testAuditTable = new List<Audit>();
            var fakeAuditDbSet = Aef.FakeDbSet(this.testAuditTable);
            A.CallTo(() => this.fakeDbContext.Audit).Returns(fakeAuditDbSet);
        }

        [Fact]
        public void AddAuditTest()
        {

            // Act
            this.repo.Add(new AccountNotificationAudit() { Email = nameof(AccountNotificationAudit.Email), NotificationProcessingStatus = NotificationProcessingStatus.Completed, Note = nameof(AccountNotificationAudit.Note) });

            // Asserts
            A.CallTo(() => this.fakeDbContext.Audit.Add(A<Audit>._)).MustHaveHappened();

            this.testAuditTable.Count().Should().Be(1);

            var insertedAudit = this.testAuditTable.FirstOrDefault();
            insertedAudit.Email.Should().Be(nameof(AccountNotificationAudit.Email));
            insertedAudit.Status.Should().Be(NotificationProcessingStatus.Completed.ToString());
            insertedAudit.Notes.Should().Be(nameof(AccountNotificationAudit.Note));
        }

        [Fact]
        public void SetAuditBatchToProcessingTest()
        {
            // Setup
            var accounts = this.SetUpTestAccounts();

            // Act
            this.repo.SetBatchToProcessing(accounts);

            // Asserts
            A.CallTo(() => this.fakeDbContext.Audit.Add(A<Audit>._)).MustHaveHappenedTwiceExactly();
            this.CheckExpectedResults(accounts, NotificationProcessingStatus.InProgress.ToString());
        }

        [Fact]
        public void SetAuditBatchToCircuitGotBrokenTest()
        {
            // Setup
            var accounts = this.SetUpTestAccounts();

            // Act
            this.repo.SetBatchToProcessing(accounts);
            this.repo.SetBatchToCircuitGotBroken(accounts);

            // Asserts
            this.CheckExpectedResults(accounts, NotificationProcessingStatus.CircuitGotBroken.ToString());
        }

        private void CheckExpectedResults(List<Account> accounts, string expectedStatus)
        {
            this.testAuditTable.Count().Should().Be(accounts.Count);

            var insertedAudit1 = this.testAuditTable.FirstOrDefault();
            var account1 = accounts.FirstOrDefault();
            insertedAudit1.Email.Should().Be(account1.EMail);
            insertedAudit1.Status.Should().Be(expectedStatus);

            var insertedAudit2 = this.testAuditTable.TakeLast(1).FirstOrDefault();
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