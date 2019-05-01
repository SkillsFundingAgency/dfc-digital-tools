using Autofac;

namespace DFC.Digital.Tools.Service.Accounts
{
    public class AccountsServiceAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(this.ThisAssembly).AsImplementedInterfaces();
        }
    }
}