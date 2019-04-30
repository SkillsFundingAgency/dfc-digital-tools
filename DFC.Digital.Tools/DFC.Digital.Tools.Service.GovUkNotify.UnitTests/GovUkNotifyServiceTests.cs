using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Data.Models;
using FakeItEasy;
using FluentAssertions;
using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Digital.Tools.Service.GovUkNotify.UnitTests
{
    public class GovUkNotifyServiceTests
    {
        private static readonly SendNotificationResponse FailedResponseWithRateException =
            new SendNotificationResponse
            {
                RateLimitException = true,
                Success = false
            };

        private static readonly SendNotificationResponse FailedResponse =
            new SendNotificationResponse
            {
                Success = false
            };

        private static readonly SendNotificationResponse SuccessResponse =
            new SendNotificationResponse
            {
                Success = true
            };

        private readonly IApplicationLogger fakeApplicationLogger;
        private readonly IGovUkNotifyClientProxy fakeGovUkNotifyClient;
        private readonly IConfigConfigurationProvider fakeConfiguration;
        private readonly ICircuitBreakerRepository fakeCircuitBreakerRepository;

    public GovUkNotifyServiceTests()
        {
            fakeApplicationLogger = A.Fake<IApplicationLogger>(ops => ops.Strict());
            fakeConfiguration = A.Fake<IConfigConfigurationProvider>(ops => ops.Strict());
            fakeGovUkNotifyClient = A.Fake<IGovUkNotifyClientProxy>(ops => ops.Strict());
            fakeCircuitBreakerRepository = A.Fake<ICircuitBreakerRepository>(ops => ops.Strict());
            SetupCalls();
        }

        public static IEnumerable<object[]> SendCitizenNotificationInput()
        {
            yield return new object[]
            {
                "1",
                false,
                false,
                SuccessResponse
            };
            yield return new object[]
            {
                "1",
                true,
                true,
                FailedResponseWithRateException
            };
            yield return new object[]
            {
                "1",
                true,
                false,
                FailedResponse
            };
            yield return new object[]
            {
                null,
                false,
                false,
                FailedResponse
            };
        }

        [Theory]
        [InlineData(nameof(CitizenNotification.Firstname), nameof(CitizenNotification.Firstname), nameof(CitizenNotification.Firstname))]
        [InlineData(nameof(CitizenNotification.Firstname), null, Constants.UnknownValue)]
        [InlineData(nameof(CitizenNotification.Firstname), "", Constants.UnknownValue)]
        public void ConvertTest(string key, string sourceValue, string expectedValue)
        {
            // Arrange
            var input = new GovUkNotifyPersonalisation
            {
                Personalisation = new Dictionary<string, string>
                {
                    { key, sourceValue }
                }
            };

            var expectation = new Dictionary<string, dynamic>
            {
                { key, expectedValue }
            };

            // Act
            var govUkNotifyService = new GovUkNotifyService(fakeApplicationLogger, fakeGovUkNotifyClient, fakeConfiguration, fakeCircuitBreakerRepository);
            var result = govUkNotifyService.Convert(input);

            // Assert
            result.Should().BeEquivalentTo(expectation);
        }

        [Theory]
        [MemberData(nameof(SendCitizenNotificationInput))]
        public async Task SendCitizenNotificationAsyncTests(string responseId, bool throwException, bool isRateLimitException, SendNotificationResponse expectation)
        {
            //Fakes
            var citizenEmailNotification = new CitizenEmailNotification
            {
                EmailAddress = "dumy@email.com"
            };
            var emailResponse = responseId == null ? null : new Notify.Models.Responses.EmailNotificationResponse
            {
                id = responseId
            };

            //Configure calls
            if (throwException)
            {
                A.CallTo(() => fakeGovUkNotifyClient.SendEmail(A<string>._, A<string>._, A<string>._, A<Dictionary<string, dynamic>>._)).Throws(() => new NotifyClientException(isRateLimitException ? nameof(NotifyClientException).ToLowerInvariant() : nameof(Exception).ToLowerInvariant()));
            }
            else
            {
                A.CallTo(() => fakeGovUkNotifyClient.SendEmail(A<string>._, A<string>._, A<string>._, A<Dictionary<string, dynamic>>._)).Returns(emailResponse);
            }

            A.CallTo(() => fakeConfiguration.GetConfigSectionKey<string>(A<string>._, A<string>._)).Returns(isRateLimitException ? nameof(NotifyClientException).ToLowerInvariant() : "test");

            //Act
            var govUkNotifyService = new GovUkNotifyService(fakeApplicationLogger, fakeGovUkNotifyClient, fakeConfiguration, fakeCircuitBreakerRepository);
            var result = await govUkNotifyService.SendCitizenNotificationAsync(citizenEmailNotification);

            //Assertions
            result.Should().BeEquivalentTo(expectation);
            A.CallTo(() => fakeGovUkNotifyClient.SendEmail(A<string>._, A<string>.That.IsEqualTo(citizenEmailNotification.EmailAddress), A<string>._, A<Dictionary<string, dynamic>>._)).MustHaveHappened();
            if (throwException)
            {
                A.CallTo(() => fakeApplicationLogger.Error(A<string>._, A<Exception>._)).MustHaveHappened();
                if (isRateLimitException)
                {
                    A.CallTo(() => fakeCircuitBreakerRepository.OpenCircuitBreakerAsync()).MustHaveHappened();
                }
            }
        }

        private void SetupCalls()
        {
            A.CallTo(() => fakeApplicationLogger.Error(A<string>._, A<Exception>._)).DoesNothing();
            A.CallTo(() => fakeCircuitBreakerRepository.OpenCircuitBreakerAsync()).Returns(Task.CompletedTask);
        }
    }
}