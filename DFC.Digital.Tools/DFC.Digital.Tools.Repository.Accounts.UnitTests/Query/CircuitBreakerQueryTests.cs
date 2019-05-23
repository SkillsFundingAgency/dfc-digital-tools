using DFC.Digital.Tools.Data.Models;
using DFC.Digital.Tools.Repository.Accounts.Query;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Testing.FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Digital.Tools.Repository.Accounts.UnitTests
{
    public class CircuitBreakerQueryTests
    {
        [Fact]
        public void GetAccountsThatStillNeedProcessingDateTest()
        {
            // Setup
            var fakeDbContext = A.Fake<DFCUserAccountsContext>();
            var repo = new CircuitBreakerQuery(fakeDbContext);

            var lastCircuitOpenDate = DateTime.Now;

            List<CircuitBreaker> testCircuitBreakerData = new List<CircuitBreaker>
            {
                new CircuitBreaker { CircuitBreakerStatus = CircuitBreakerStatus.Open.ToString(), HalfOpenRetryCount = 1, LastCircuitOpenDate = lastCircuitOpenDate },
                new CircuitBreaker { CircuitBreakerStatus = CircuitBreakerStatus.Closed.ToString(), HalfOpenRetryCount = 0, LastCircuitOpenDate = lastCircuitOpenDate },
            };

            var fakeCircuitBreakerDbSet = Aef.FakeDbSet(testCircuitBreakerData);
            A.CallTo(() => fakeDbContext.CircuitBreaker).Returns(fakeCircuitBreakerDbSet);

            // Act
            var result = repo.GetBreakerDetails();

            // Assert
            result.Should().NotBeNull();
            result.HalfOpenRetryCount.Should().Be(1);
            result.CircuitBreakerStatus.Should().Be(CircuitBreakerStatus.Open);
            result.LastCircuitOpenDate.Should().Be(lastCircuitOpenDate);
        }
    }
}