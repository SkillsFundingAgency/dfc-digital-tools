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
           fakeDbContext = A.Fake<DFCUserAccountsContext>();
           repo = new CircuitBreakerCommandRepository(this.fakeDbContext);
           testCircuitBreakerTable = new List<CircuitBreaker>();
           var fakeCircuitBreakerDbSet = Aef.FakeDbSet(this.testCircuitBreakerTable);
           A.CallTo(() => fakeDbContext.CircuitBreaker).Returns(fakeCircuitBreakerDbSet);
        }

        [Fact]
        public void AddAuditTest()
        {
            // Setup
            var lastCircuitOpenDate = DateTime.Now;
            var testCircuitBreaker = new CircuitBreakerDetails() { CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen, HalfOpenRetryCount = 5, LastCircuitOpenDate = lastCircuitOpenDate };

            // Act
            repo.Add(testCircuitBreaker);

            // Asserts
            A.CallTo(() => fakeDbContext.CircuitBreaker.Add(A<CircuitBreaker>._)).MustHaveHappened();

            testCircuitBreakerTable.Count().Should().Be(1);

            var insertedCircuitBreaker = testCircuitBreakerTable.FirstOrDefault();
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
               testCircuitBreakerTable.Add(new CircuitBreaker());
            }

            var result = repo.UpdateIfExistsAsync(testCircuitBreaker).Result;

            // Asserts
            result.Should().Be(circuitBreakerRecordExists);

            if (circuitBreakerRecordExists)
            {
                var insertedCircuitBreaker = testCircuitBreakerTable.FirstOrDefault();
                insertedCircuitBreaker.CircuitBreakerStatus.Should().Be(testCircuitBreaker.CircuitBreakerStatus.ToString());
                insertedCircuitBreaker.HalfOpenRetryCount.Should().Be(testCircuitBreaker.HalfOpenRetryCount);
                insertedCircuitBreaker.LastCircuitOpenDate.Should().Be(testCircuitBreaker.LastCircuitOpenDate);
            }
        }
    }
}