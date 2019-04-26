using Autofac;

namespace DFC.Digital.Tools.Repository.Pirean
{
    public class AccountsAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}
