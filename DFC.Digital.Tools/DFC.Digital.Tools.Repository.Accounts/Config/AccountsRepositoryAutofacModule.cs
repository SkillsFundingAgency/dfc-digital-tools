using Autofac;
using DFC.Digital.Tools.Core;
using DFC.Digital.Tools.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public class AccountsRepositoryAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
            builder.Register(c =>
            {
                var config = c.Resolve<IConfigConfigurationProvider>();
                var opt = new DbContextOptionsBuilder<DFCUserAccountsContext>();
                opt.UseSqlServer(config.GetConfigSectionKey<string>(Constants.AccountRepositorySection, Constants.SQLConnection));
                return new DFCUserAccountsContext(opt.Options);
            }).InstancePerLifetimeScope();
        }
    }
}
