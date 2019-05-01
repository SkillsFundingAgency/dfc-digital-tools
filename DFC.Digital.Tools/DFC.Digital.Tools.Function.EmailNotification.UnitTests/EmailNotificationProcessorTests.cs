using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ICitizenNotificationRepository<CitizenEmailNotification> fakeCitizenEmailRepository;
        private readonly IApplicationLogger fakeApplicationLogger;
        private readonly ICircuitBreakerRepository fakeCircuitBreakerRepository;
        private readonly IConfigConfigurationProvider fakeConfiguration;
        private readonly IAccountsService fakeAccountsService;

        public EmailNotificationProcessorTests()
        {
            fakeCircuitBreakerRepository = A.Fake<ICircuitBreakerRepository>(ops => ops.Strict());
            fakeAccountsService = A.Fake<IAccountsService>(ops => ops.Strict());
            fakeApplicationLogger = A.Fake<IApplicationLogger>(ops => ops.Strict());
            fakeSendCitizenNotificationService =
                A.Fake<ISendCitizenNotification<Account>>(ops => ops.Strict());
            fakeConfiguration = A.Fake<IConfigConfigurationProvider>(ops => ops.Strict());
            fakeCitizenEmailRepository =
                A.Fake<ICitizenNotificationRepository<CitizenEmailNotification>>(ops => ops.Strict());
            SetupCalls();
        }

        public static IEnumerable<object[]> GetProcessEmailNotificationInput()
        {
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
            A.CallTo(() => fakeCircuitBreakerRepository.GetCircuitBreakerStatusAsync()).Returns(circuitBreakerDetails);
            A.CallTo(() => fakeAccountsService.GetNextBatchOfEmailsAsync(A<int>._)).Returns(GetAccountsToProcess(batchAccountSize));
            A.CallTo(() => fakeCitizenEmailRepository.GetCitizenEmailNotificationsAsync()).Returns(GetEmailNotifications(batchAccountSize));
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
            var emailProcessor = new EmailNotificationProcessor(fakeSendCitizenNotificationService, fakeCitizenEmailRepository, fakeApplicationLogger, fakeCircuitBreakerRepository, fakeConfiguration, fakeAccountsService);

            // Act
             await emailProcessor.ProcessEmailNotificationsAsync();

            // Assert
            A.CallTo(() => fakeCircuitBreakerRepository.GetCircuitBreakerStatusAsync()).MustHaveHappened();

            if (circuitBreakerDetails.CircuitBreakerStatus != CircuitBreakerStatus.Open)
            {
                if (throwSendNotificationException)
                {
                    A.CallTo(() => fakeCircuitBreakerRepository.HalfOpenCircuitBreakerAsync()).MustHaveHappened();
                    if (circuitBreakerDetails.HalfOpenRetryCount == halfOpenRetryMax)
                    {
                        A.CallTo(() => fakeCitizenEmailRepository.ResetCitizenEmailNotificationAsync(A<IQueryable<CitizenEmailNotification>>._)).MustHaveHappened();

                        A.CallTo(() => fakeCircuitBreakerRepository.OpenCircuitBreakerAsync()).MustHaveHappened();
                    }
                }
                else
                {
                    if (sendNotificationResponse.Success)
                    {
                        A.CallTo(() =>
                            fakeCitizenEmailRepository.UpdateCitizenEmailNotificationAsync(
                                A<CitizenEmailNotification>._)).MustHaveHappened(batchAccountSize, Times.Exactly);
                    }
                    else
                    {
                        if (sendNotificationResponse.RateLimitException)
                        {
                            A.CallTo(() => fakeCircuitBreakerRepository.OpenCircuitBreakerAsync()).MustHaveHappened();
                            A.CallTo(() =>
                                    fakeCitizenEmailRepository.ResetCitizenEmailNotificationAsync(
                                        A<IQueryable<CitizenEmailNotification>>._))
                                .MustHaveHappened();
                            A.CallTo(() => fakeApplicationLogger.Info(A<string>._)).MustHaveHappened();
                        }
                        else
                        {
                            A.CallTo(() =>
                                fakeCitizenEmailRepository.UpdateCitizenEmailNotificationAsync(
                                    A<CitizenEmailNotification>._)).MustHaveHappened();
                        }
                    }
                }
            }
            else
            {
                A.CallTo(() => fakeAccountsService.GetNextBatchOfEmailsAsync(A<int>._)).MustNotHaveHappened();
                A.CallTo(() => fakeCitizenEmailRepository.GetCitizenEmailNotificationsAsync()).MustNotHaveHappened();
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

        private static IQueryable<CitizenEmailNotification> GetEmailNotifications(int batchSize)
        {
            var accountList = new List<CitizenEmailNotification>();
            for (var i = 0; i < batchSize; i++)
            {
                accountList.Add(new CitizenEmailNotification
                {
                    EmailAddress = $"{nameof(CitizenEmailNotification.EmailAddress)} - {i}"
                });
            }

            return accountList.AsQueryable();
        }

        private void SetupCalls()
        {
            A.CallTo(() => fakeApplicationLogger.Info(A<string>._)).DoesNothing();
            A.CallTo(() => fakeApplicationLogger.Error(A<string>._, A<Exception>._)).DoesNothing();
            A.CallTo(() => fakeApplicationLogger.Trace(A<string>._)).DoesNothing();
            A.CallTo(() => fakeCircuitBreakerRepository.OpenCircuitBreakerAsync()).Returns(Task.CompletedTask);
            A.CallTo(() => fakeCircuitBreakerRepository.HalfOpenCircuitBreakerAsync()).Returns(Task.CompletedTask);
            A.CallTo(() => fakeCitizenEmailRepository.UpdateCitizenEmailNotificationAsync(A<CitizenEmailNotification>._)).Returns(Task.CompletedTask);
            A.CallTo(() => fakeCitizenEmailRepository.ResetCitizenEmailNotificationAsync(A<IQueryable<CitizenEmailNotification>>._)).Returns(Task.CompletedTask);
        }
    }
}
