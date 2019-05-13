using Autofac;
using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using DFC.Digital.Tools.Repository.Accounts;
using DFC.Digital.Tools.Service.Accounts;
using DFC.Digital.Tools.Service.GovUkNotify;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Function.EmailNotification
{
    [ExcludeFromCodeCoverage]
    public static class Startup
    {
        public static ILifetimeScope ConfigureContainer(RunMode mode, string basePath)
        {
            var builder = ConfigureDI.ConfigureContainerWithCommonModules(mode, basePath);
            builder.RegisterModule<EmailNotificationAutofacModule>();
            builder.RegisterModule<AccountsRepositoryAutofacModule>();
            builder.RegisterModule<GovUkNotifyAutofacModule>();
            builder.RegisterModule<AccountsServiceAutofacModule>();
            return builder.Build().BeginLifetimeScope();
        }

        public static async Task RunAsync(RunMode mode, string basePath)
        {
            var container = ConfigureContainer(mode, basePath);

            ConfigureLog.ConfigureNLogWithAppInsightsTarget(container.Resolve<IConfigConfigurationProvider>());
            var processEmailNotificationsService = container.Resolve<IProcessEmailNotifications>();
            await processEmailNotificationsService.ProcessEmailNotificationsAsync();
        }
    }
}