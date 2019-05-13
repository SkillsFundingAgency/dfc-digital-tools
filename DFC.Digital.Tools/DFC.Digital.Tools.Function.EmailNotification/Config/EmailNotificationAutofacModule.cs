using Autofac;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Digital.Tools.Function.EmailNotification
{
    [ExcludeFromCodeCoverage]
    public class EmailNotificationAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
