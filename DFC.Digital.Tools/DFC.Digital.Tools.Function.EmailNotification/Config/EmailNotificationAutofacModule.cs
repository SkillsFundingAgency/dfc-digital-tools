using Autofac;

namespace DFC.Digital.Tools.Function.EmailNotification
{
    public class EmailNotificationAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
