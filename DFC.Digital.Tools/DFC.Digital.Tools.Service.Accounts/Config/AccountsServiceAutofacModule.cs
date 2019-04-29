using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Service.Accounts
{
    public class AccountsServiceAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();
        }
    }
}