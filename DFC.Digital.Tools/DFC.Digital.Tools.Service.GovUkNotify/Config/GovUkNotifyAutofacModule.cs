using Autofac;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Digital.Tools.Service.GovUkNotify
{
    [ExcludeFromCodeCoverage]
    public class GovUkNotifyAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
