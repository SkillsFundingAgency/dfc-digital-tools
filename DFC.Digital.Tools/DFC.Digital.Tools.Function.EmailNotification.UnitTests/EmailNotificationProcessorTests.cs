using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Digital.Tools.Function.EmailNotification.UnitTests
{
    public class EmailNotificationProcessorTests
    {
        private static readonly CircuitBreakerDetails ClosedCircuitBreakerDetails =
            new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Closed
            };

        private static readonly CircuitBreakerDetails OpenCircuitBreakerDetails =
            new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.Open
            };

        private static readonly CircuitBreakerDetails HalfOpenCircuitBreakerDetails =
            new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen,
                HalfOpenRetryCount = 4
            };

        private static readonly CircuitBreakerDetails LastAttemptHalfOpenCircuitBreakerDetails =
            new CircuitBreakerDetails
            {
                CircuitBreakerStatus = CircuitBreakerStatus.HalfOpen,
                HalfOpenRetryCount = 5
            };

        private static readonly SendNotificationResponse SuccesSendNotificationResponse =
            new SendNotificationResponse
            {
                Success = true,
            };

        private static readonly SendNotificationResponse FailedNotificationResponse =
            new SendNotificationResponse
            {
            };

        private static readonly SendNotificationResponse FailedWithRateLimitdErrorResponse =
            new SendNotificationResponse
            {
                RateLimitException = true,
            };

        private readonly ISendCitizenNotification<Account> fakeSendCitizenNotificationService;
        private readonly IApplicationLogger fakeApplicationLogger;
        private readonly IConfigConfigurationProvider fakeConfiguration;
        private readonly IAccountsService fakeAccountsService;

        public EmailNotificationProcessorTests()
        {
            fakeAccountsService = A.Fake<IAccountsService>(ops => ops.Strict());
            fakeApplicationLogger = A.Fake<IApplicationLogger>(ops => ops.Strict());
            fakeSendCitizenNotificationService =
                A.Fake<ISendCitizenNotification<Account>>(ops => ops.Strict());
            fakeConfiguration = A.Fake<IConfigConfigurationProvider>(ops => ops.Strict());
            SetupCalls();
        }

        public static IEnumerable<object[]> GetProcessEmailNotificationInput()
        {
            yield return new object[]
            {
                HalfOpenCircuitBreakerDetails,
                150,
                SuccesSendNotificationResponse,
                2
            };
            yield return new object[]
            {
                ClosedCircuitBreakerDetails,
                150,
                SuccesSendNotificationResponse,
                5
            };
            yield return new object[]
            {
                ClosedCircuitBreakerDetails,
                150,
                FailedNotificationResponse
            };
            yield return new object[]
            {
                ClosedCircuitBreakerDetails,
                150,
                FailedNotificationResponse,
                true
            };
            yield return new object[]
            {
                ClosedCircuitBreakerDetails,
                150,
                FailedWithRateLimitdErrorResponse
            };
            yield return new object[]
            {
                LastAttemptHalfOpenCircuitBreakerDetails,
                150,
                FailedNotificationResponse,
                true
            };
            yield return new object[]
            {
                HalfOpenCircuitBreakerDetails,
                150,
                SuccesSendNotificationResponse
            };
            yield return new object[]
            {
                HalfOpenCircuitBreakerDetails,
                150,
                FailedNotificationResponse
            };
            yield return new object[]
            {
                HalfOpenCircuitBreakerDetails,
                150,
                FailedWithRateLimitdErrorResponse
            };
            yield return new object[]
            {
                OpenCircuitBreakerDetails,
                150,
                FailedNotificationResponse
            };
           }

        [Theory]
        [MemberData(nameof(GetProcessEmailNotificationInput))]
        public async Task ProcessEmailNotificationsAsyncTests(
            CircuitBreakerDetails circuitBreakerDetails,
            int batchAccountSize,
            SendNotificationResponse sendNotificationResponse,
            bool throwSendNotificationException = false,
            int halfOpenRetryMax = 5)
        {
            // Configure Calls
            A.CallTo(() => fakeAccountsService.GetCircuitBreakerStatusAsync()).Returns(circuitBreakerDetails);
            A.CallTo(() => fakeAccountsService.GetNextBatchOfEmailsAsync(A<int>._)).Returns(GetAccountsToProcess(batchAccountSize));
            if (throwSendNotificationException)
            {
                A.CallTo(() => fakeSendCitizenNotificationService.SendCitizenNotificationAsync(A<Account>._)).Throws<Exception>();
            }
            else
            {
                A.CallTo(() => fakeSendCitizenNotificationService.SendCitizenNotificationAsync(A<Account>._)).Returns(sendNotificationResponse);
            }

            A.CallTo(() => fakeConfiguration.GetConfigSectionKey<int>(A<string>._, A<string>._))
                .Returns(halfOpenRetryMax);

            // Assign
            var emailProcessor = new EmailNotificationProcessor(fakeSendCitizenNotificationService, fakeApplicationLogger, fakeConfiguration, fakeAccountsService);

            // Act
             await emailProcessor.ProcessEmailNotificationsAsync();

            // Assert
            A.CallTo(() => fakeAccountsService.GetCircuitBreakerStatusAsync()).MustHaveHappened();

            if (circuitBreakerDetails.CircuitBreakerStatus != CircuitBreakerStatus.Open)
            {
                if (throwSendNotificationException)
                {
                    A.CallTo(() =>
                            fakeAccountsService.InsertAuditAsync(A<AccountNotificationAudit>.That.Matches(audit =>
                                audit.NotificationProcessingStatus == NotificationProcessingStatus.ExceptionOccured)))
                        .MustHaveHappened();
                    A.CallTo(() => fakeAccountsService.HalfOpenCircuitBreakerAsync()).MustHaveHappened();
                    if (circuitBreakerDetails.HalfOpenRetryCount == halfOpenRetryMax)
                    {
                        A.CallTo(() => fakeAccountsService.SetBatchToCircuitGotBrokenAsync(A<IEnumerable<Account>>._)).MustHaveHappened();

                        A.CallTo(() => fakeAccountsService.OpenCircuitBreakerAsync()).MustHaveHappened();
                    }
                }
                else
                {
                    if (sendNotificationResponse.Success)
                    {
                        A.CallTo(() =>
                            fakeAccountsService.InsertAuditAsync(A<AccountNotificationAudit>._)).MustHaveHappened(batchAccountSize, Times.Exactly);
                        if (circuitBreakerDetails.CircuitBreakerStatus == CircuitBreakerStatus.HalfOpen)
                        {
                            A.CallTo(() =>
                                fakeAccountsService.CloseCircuitBreakerAsync()).MustHaveHappened();
                        }
                    }
                    else
                    {
                        if (sendNotificationResponse.RateLimitException)
                        {
                            A.CallTo(() => fakeAccountsService.OpenCircuitBreakerAsync()).MustHaveHappened();
                            A.CallTo(() =>
                                    fakeAccountsService.SetBatchToCircuitGotBrokenAsync(
                                        A<IEnumerable<Account>>._))
                                .MustHaveHappened();
                            A.CallTo(() => fakeApplicationLogger.Info(A<string>._)).MustHaveHappened();
                        }
                        else
                        {
                            A.CallTo(() =>
                                fakeAccountsService.InsertAuditAsync(A<AccountNotificationAudit>._)).MustHaveHappened();
                        }
                    }
                }
            }
            else
            {
                A.CallTo(() => fakeAccountsService.GetNextBatchOfEmailsAsync(A<int>._)).MustNotHaveHappened();
                A.CallTo(() => fakeSendCitizenNotificationService.SendCitizenNotificationAsync(A<Account>._)).MustNotHaveHappened();
            }
        }

        private static IEnumerable<Account> GetAccountsToProcess(int batchSize)
        {
            var accountList = new List<Account>();
            for (var i = 0; i < batchSize; i++)
            {
                accountList.Add(new Account
                {
                    EMail = nameof(Account.EMail),
                    Name = $"{nameof(Account.Name)} - {i}"
                });
            }

            return accountList;
        }

        private void SetupCalls()
        {
            A.CallTo(() => fakeApplicationLogger.Info(A<string>._)).DoesNothing();
            A.CallTo(() => fakeApplicationLogger.ErrorJustLogIt(A<string>._, A<Exception>._)).DoesNothing();
            A.CallTo(() => fakeApplicationLogger.Trace(A<string>._)).DoesNothing();
            A.CallTo(() => fakeAccountsService.OpenCircuitBreakerAsync()).Returns(Task.CompletedTask);
            A.CallTo(() => fakeAccountsService.HalfOpenCircuitBreakerAsync()).Returns(Task.CompletedTask);
            A.CallTo(() => fakeAccountsService.InsertAuditAsync(A<AccountNotificationAudit>._)).Returns(Task.CompletedTask);
            A.CallTo(() => fakeAccountsService.SetBatchToCircuitGotBrokenAsync(A<IEnumerable<Account>>._)).Returns(Task.CompletedTask);
            A.CallTo(() => fakeAccountsService.CloseCircuitBreakerAsync()).Returns(Task.CompletedTask);
        }
    }
}
