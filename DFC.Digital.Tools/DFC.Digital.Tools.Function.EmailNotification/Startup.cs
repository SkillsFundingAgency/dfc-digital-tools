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
        public static ILifetimeScope ConfigureContainer(RunMode mode)
        {
            var builder = ConfigureDI.ConfigureContainerWithCommonModules(mode);
            builder.RegisterModule<EmailNotificationAutofacModule>();
            builder.RegisterModule<AccountsRepositoryAutofacModule>();
            builder.RegisterModule<GovUkNotifyAutofacModule>();
            builder.RegisterModule<AccountsServiceAutofacModule>();
            return builder.Build().BeginLifetimeScope();
        }

        public static async Task RunAsync(RunMode mode)
        {
            var container = ConfigureContainer(mode);
            var processEmailNotificationsService = container.Resolve<IProcessEmailNotifications>();
            await processEmailNotificationsService.ProcessEmailNotificationsAsync();
        }
    }
}