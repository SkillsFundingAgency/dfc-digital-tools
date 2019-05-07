using DFC.Digital.Tools.Data.Models;
using DFC.Digital.Tools.Repository.Accounts.Command;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Testing.FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Digital.Tools.Repository.Accounts.UnitTests
{
    public class CircuitBreakerCommandRepositoryTests
    {
        private readonly DFCUserAccountsContext fakeDbContext;
        private readonly CircuitBreakerCommandRepository repo;
        private readonly List<CircuitBreaker> testCircuitBreakerTable;

        public CircuitBreakerCommandRepositoryTests()
        {
            this.fakeDbContext = A.Fake<DFCUserAccountsContext>();
            this.repo = new CircuitBreakerCommandRepository(this.fakeDbContext);
            this.testCircuitBreakerTable = new List<CircuitBreaker>();
            var fakeCircuitBreakerDbSet = Aef.FakeDbSet(this.testCircuitBreakerTable);
            A.CallTo(() => this.fakeDbContext.CircuitBreaker).Returns(fakeCircuitBreakerDbSet);
        }

        [Fact]
        public void AddAuditTest()
        {
            // Setup
            var lastCircuitOpenDate = DateTime.Now;
            var testCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen, HalfOpenRetryCount = 5, LastCircuitOpenDate = lastCircuitOpenDate };

            // Act
            this.repo.Add(testCircuitBreaker);

            // Asserts
            A.CallTo(() => this.fakeDbContext.CircuitBreaker.Add(A<CircuitBreaker>._)).MustHaveHappened();

            this.testCircuitBreakerTable.Count().Should().Be(1);

            var insertedCircuitBreaker = this.testCircuitBreakerTable.FirstOrDefault();
            insertedCircuitBreaker.CircuitBreakerStatus.Should().Be(testCircuitBreaker.CircuitBreakerStatus.ToString());
            insertedCircuitBreaker.HalfOpenRetryCount.Should().Be(testCircuitBreaker.HalfOpenRetryCount);
            insertedCircuitBreaker.LastCircuitOpenDate.Should().Be(testCircuitBreaker.LastCircuitOpenDate);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateIfExistsTest(bool circuitBreakerRecordExists)
        {
            // Setup
            var lastCircuitOpenDate = DateTime.Now;
            var testCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen, HalfOpenRetryCount = 5, LastCircuitOpenDate = lastCircuitOpenDate };

            if (circuitBreakerRecordExists)
            {
                this.testCircuitBreakerTable.Add(new CircuitBreaker());
            }

            var result = this.repo.UpdateIfExists(testCircuitBreaker);

            // Asserts
            result.Should().Be(circuitBreakerRecordExists);

            if (circuitBreakerRecordExists)
            {
                A.CallTo(() => this.fakeDbContext.SaveChanges()).MustHaveHappened();

                var insertedCircuitBreaker = this.testCircuitBreakerTable.FirstOrDefault();
                insertedCircuitBreaker.CircuitBreakerStatus.Should().Be(testCircuitBreaker.CircuitBreakerStatus.ToString());
                insertedCircuitBreaker.HalfOpenRetryCount.Should().Be(testCircuitBreaker.HalfOpenRetryCount);
                insertedCircuitBreaker.LastCircuitOpenDate.Should().Be(testCircuitBreaker.LastCircuitOpenDate);
            }
        }
    }
}