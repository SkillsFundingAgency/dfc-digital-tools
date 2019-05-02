namespace DFC.Digital.Tools.Repository.Accounts.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DFC.Digital.Tools.Data.Models;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Testing.FakeItEasy;
    using Xunit;

    public class AccountsQueryTests
    {
        [Fact]
        public void GetAccountsThatStillNeedProcessingDateTest()
        {
            // Setup
            var fakeDbContext = A.Fake<DFCUserAccountsContext>();
            var repo = new AccountsQuery(fakeDbContext);

            var cutOfDate = this.ConvertToDateTime("01-Nov-2018");

            List<Accounts> testAccountData = new List<Accounts>
            {
                new Accounts { Name = "Well Before Date", Mail = "TestMail1@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate.AddDays(-50) },
                new Accounts { Name = "One Day Before Date", Mail = "TestMail2@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate.AddDays(-1) },
                new Accounts { Name = "On Cut Off Date", Mail = "TestMail3@dummy.com", Uid = "TestMail3@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "After Cut Off Daye", Mail = "TestMail4@dummy.com", Uid = "TestMail4@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate.AddDays(1) }
            };
            var fakeDbSet = Aef.FakeDbSet(testAccountData);
            A.CallTo(() => fakeDbContext.Accounts).Returns(fakeDbSet);

            List<Audit> testAuditData = new List<Audit>();
            var fakeAuditDbSet = Aef.FakeDbSet(testAuditData);
            A.CallTo(() => fakeDbContext.Audit).Returns(fakeAuditDbSet);

            // Act
            var results = repo.GetAccountsThatStillNeedProcessing(cutOfDate);

            // Assert
            results.Count().Should().Be(2);
        }

        [Fact]
        public void GetAccountsThatStillNeedProcessingProcessingStatesTest()
        {
            // Setup
            var fakeDbContext = A.Fake<DFCUserAccountsContext>();
            var repo = new AccountsQuery(fakeDbContext);

            var cutOfDate = this.ConvertToDateTime("31-Oct-2018");

            List<Accounts> testAccountData = new List<Accounts>
            {
                new Accounts { Name = "Not In Audit", Mail = "TestMail1@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "In Audit - Processing", Mail = "TestMail2@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "In Audit - Circuit Got Broken", Mail = "TestMail3@dummy.com", Uid = "TestMail3@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "In Audit - Failed", Mail = "TestMail4@dummy.com", Uid = "TestMail4@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "In Audit - Completed", Mail = "TestMail5@dummy.com", Uid = "TestMail5@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate }
            };
            var fakeDbSet = Aef.FakeDbSet(testAccountData);
            A.CallTo(() => fakeDbContext.Accounts).Returns(fakeDbSet);

            List<Audit> testAuditData = new List<Audit>
            {
                new Audit { Email = "TestMail2@dummy.com", Status = NotificationProcessingStatus.InProgress.ToString() },
                new Audit { Email = "TestMail3@dummy.com", Status = NotificationProcessingStatus.CircuitGotBroken.ToString() },
                new Audit { Email = "TestMail4@dummy.com", Status = NotificationProcessingStatus.InProgress.ToString() },
                new Audit { Email = "TestMail4@dummy.com", Status = NotificationProcessingStatus.Failed.ToString() },
                new Audit { Email = "TestMail5@dummy.com", Status = NotificationProcessingStatus.InProgress.ToString() },
                new Audit { Email = "TestMail5@dummy.com", Status = NotificationProcessingStatus.Completed.ToString() }
            };

            var fakeAuditDbSet = Aef.FakeDbSet(testAuditData);
            A.CallTo(() => fakeDbContext.Audit).Returns(fakeAuditDbSet);

            // Act
            var results = repo.GetAccountsThatStillNeedProcessing(cutOfDate.AddDays(1));

            // Assert should have the one that not in Audit and the broken circuit one
            results.Count().Should().Be(2);
        }

        [Fact]
        public void GetAccountsThatStillNeedProcessingStatesTest()
        {
            // Setup
            var fakeDbContext = A.Fake<DFCUserAccountsContext>();
            var repo = new AccountsQuery(fakeDbContext);

            var cutOfDate = this.ConvertToDateTime("31-Oct-2018");

            List<Accounts> testAccountData = new List<Accounts>
            {
                new Accounts { Name = "ACTIVATED", Mail = "TestMail1@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "ACTIVATED", Createtimestamp = cutOfDate },
                new Accounts { Name = "PENDINGACTIVATION", Mail = "TestMail2@dummy.com", Uid = "TestMail1@dummy.com", A1lifecycleStateUpin = "PENDINGACTIVATION", Createtimestamp = cutOfDate },
                new Accounts { Name = "INITIAL", Mail = "TestMail3@dummy.com", Uid = "TestMail3@dummy.com", A1lifecycleStateUpin = "INITIAL", Createtimestamp = cutOfDate },
                new Accounts { Name = "PENDINGRESET", Mail = "TestMail4@dummy.com", Uid = "TestMail4@dummy.com", A1lifecycleStateUpin = "PENDINGRESET", Createtimestamp = cutOfDate },
            };
            var fakeDbSet = Aef.FakeDbSet(testAccountData);
            A.CallTo(() => fakeDbContext.Accounts).Returns(fakeDbSet);

            List<Audit> testAuditData = new List<Audit>();
            var fakeAuditDbSet = Aef.FakeDbSet(testAuditData);
            A.CallTo(() => fakeDbContext.Audit).Returns(fakeAuditDbSet);

            // Act
            var results = repo.GetAccountsThatStillNeedProcessing(cutOfDate.AddDays(1));

            // Assert should get the one Activated and Pending Activation.
            results.Count().Should().Be(2);
        }

        private DateTime ConvertToDateTime(string date)
        {
            return Convert.ToDateTime(date, new CultureInfo("en-GB"));
        }
    }
}
