using Autofac;

namespace DFC.Digital.Tools.Repository.Pirean
{
    public class PireanAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
