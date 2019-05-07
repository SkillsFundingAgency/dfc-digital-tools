using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Digital.Tools.Service.Accounts.UnitTests
{

    public class AccountsServiceTests
    {
        private readonly IApplicationLogger fakeApplicationLogger;
        private readonly IConfigConfigurationProvider fakeConfiguration;
        private readonly IAccountQueryRepository fakeAccountQueryRepository;
        private readonly IAuditCommandRepository fakeAuditCommandRepository;
        private readonly ICircuitBreakerQueryRepository fakeCircuitBreakerQueryRepository;
        private readonly ICircuitBreakerCommandRepository fakeCircuitBreakerCommandRepository;

        public AccountsServiceTests()
        {
            this.fakeApplicationLogger = A.Fake<IApplicationLogger>(ops => ops.Strict());
            this.fakeConfiguration = A.Fake<IConfigConfigurationProvider>(ops => ops.Strict());
            this.fakeAccountQueryRepository = A.Fake<IAccountQueryRepository>(ops => ops.Strict());
            this.fakeAuditCommandRepository = A.Fake<IAuditCommandRepository>(ops => ops.Strict());
            this.fakeCircuitBreakerQueryRepository = A.Fake<ICircuitBreakerQueryRepository>(ops => ops.Strict());
            this.fakeCircuitBreakerCommandRepository = A.Fake<ICircuitBreakerCommandRepository>(ops => ops.Strict());
        }

        [Fact]
        public void GetCircuitBreakerStatusAsyncDoesNotExistTest()
        {
            // Arrange
            this.SetupCalls();
            A.CallTo(() => this.fakeCircuitBreakerQueryRepository.GetBreakerDetails()).Returns(null);
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.Add(A<CircuitBreakerDetails>._)).DoesNothing();

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.GetCircuitBreakerStatusAsync().Result;

            // Assert
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.Add(A<CircuitBreakerDetails>._)).MustHaveHappenedOnceExactly();
            result.CircuitBreakerStatus.Should().Be(CircuitBreakerStatus.Closed);
            result.HalfOpenRetryCount.Should().Be(0);
            result.LastCircuitOpenDate.Should().BeCloseTo(DateTime.Now, 1000);
        }

        [Theory]
        [InlineData(CircuitBreakerStatus.Closed, 10)]
        [InlineData(CircuitBreakerStatus.Open, 10)]
        [InlineData(CircuitBreakerStatus.Open, 25)]
        public void GetCircuitBreakerStatusAsyncDoesExistTest(CircuitBreakerStatus circuitBreakerStatus, int hoursInSate)
        {
            // Arrange
            this.SetupCalls();

            var dummyCircuitBreakerDetails = new CircuitBreakerDetails() { CircuitBreakerStatus = circuitBreakerStatus, HalfOpenRetryCount = 2, LastCircuitOpenDate = DateTime.Now.AddHours(hoursInSate * -1) };

            A.CallTo(() => this.fakeCircuitBreakerQueryRepository.GetBreakerDetails()).Returns(dummyCircuitBreakerDetails);
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).Returns(true);

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.GetCircuitBreakerStatusAsync().Result;

            // Assert
            if (circuitBreakerStatus == CircuitBreakerStatus.Open && hoursInSate > 24)
            {
                // if it was open and it been over 24 hours since then it gets reset
                A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappenedOnceExactly();
                result.CircuitBreakerStatus.Should().Be(CircuitBreakerStatus.Closed);
                result.HalfOpenRetryCount.Should().Be(0);
                result.LastCircuitOpenDate.Should().BeCloseTo(DateTime.Now, 1000);
            }
            else
            {
                A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustNotHaveHappened();
                result.Should().BeEquivalentTo(dummyCircuitBreakerDetails);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateCircuitBreakerAsyncTest(bool recordExists)
        {
            // Arrange
            this.SetupCalls();
            var dummyCircuitBreakerDetails = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.Closed, HalfOpenRetryCount = 0, LastCircuitOpenDate = DateTime.Now };

            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.Add(A<CircuitBreakerDetails>._)).DoesNothing();
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).Returns(recordExists);

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.UpdateCircuitBreakerAsync(dummyCircuitBreakerDetails);

            // Assert
            if (recordExists)
            {
                A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappenedTwiceExactly();
            }
        }

        [Fact]
        public void OpenCircuitBreakerAsyncTest()
        {
            // Arrange
            this.SetupCalls();
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.Add(A<CircuitBreakerDetails>._)).DoesNothing();
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).Returns(true);

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.OpenCircuitBreakerAsync();

            // Assert
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CloseCircuitBreakerAsyncTest()
        {
            // Arrange
            this.SetupCalls();
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.Add(A<CircuitBreakerDetails>._)).DoesNothing();
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).Returns(true);

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.CloseCircuitBreakerAsync();

            // Assert
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(CircuitBreakerStatus.HalfOpen)]
        [InlineData(CircuitBreakerStatus.Closed)]
        public void HalfOpenCircuitBreakerAsyncTest(CircuitBreakerStatus circuitBreakerStatus)
        {
            // Arrange
            this.SetupCalls();

            var dummyCircuitBreakerDetails = new CircuitBreakerDetails() { CircuitBreakerStatus = circuitBreakerStatus, HalfOpenRetryCount = 0, LastCircuitOpenDate = DateTime.Now };

            A.CallTo(() => this.fakeCircuitBreakerQueryRepository.GetBreakerDetails()).Returns(dummyCircuitBreakerDetails);
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).Returns(true);

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.HalfOpenCircuitBreakerAsync();

            // Assert
            A.CallTo(() => this.fakeCircuitBreakerCommandRepository.UpdateIfExists(A<CircuitBreakerDetails>._)).MustHaveHappened();

            dummyCircuitBreakerDetails.CircuitBreakerStatus.Should().Be(CircuitBreakerStatus.HalfOpen);

            if (circuitBreakerStatus == CircuitBreakerStatus.HalfOpen)
            {
                dummyCircuitBreakerDetails.HalfOpenRetryCount.Should().Be(1);
            }
        }

        [Fact]
        public void InsertAuditAsyncTest()
        {
            // Arrange
            this.SetupCalls();
            A.CallTo(() => this.fakeAuditCommandRepository.Add(A<AccountNotificationAudit>._)).DoesNothing();

            var dummyAudit = new AccountNotificationAudit();

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.InsertAuditAsync(dummyAudit);

            // Assert
            A.CallTo(() => this.fakeAuditCommandRepository.Add(dummyAudit)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void SetBatchToCircuitGotBrokenAsyncTest()
        {
            // Arrange
            this.SetupCalls();
            A.CallTo(() => this.fakeAuditCommandRepository.SetBatchToCircuitGotBroken(A<IList<Account>>._)).DoesNothing();
            IEnumerable<Account> fakeAccounts = A.Fake<IEnumerable<Account>>();

            // Act
            var accountService = new AccountsService(this.fakeApplicationLogger, this.fakeConfiguration, this.fakeAccountQueryRepository, this.fakeAuditCommandRepository, this.fakeCircuitBreakerQueryRepository, this.fakeCircuitBreakerCommandRepository);
            var result = accountService.SetBatchToCircuitGotBrokenAsync(fakeAccounts);

            // Assert
            A.CallTo(() => this.fakeAuditCommandRepository.SetBatchToCircuitGotBroken(A<IList<Account>>._)).MustHaveHappenedOnceExactly();
        }

        private void SetupCalls()
        {
            A.CallTo(() => this.fakeApplicationLogger.Trace(A<string>._)).DoesNothing();
        }
    }
}
