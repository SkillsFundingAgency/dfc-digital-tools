using Autofac;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    public class GovUkNotifyAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
