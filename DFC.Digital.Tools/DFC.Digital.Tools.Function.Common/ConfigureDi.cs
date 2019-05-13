using Autofac;
using DFC.Digital.Tools.Core;

namespace DFC.Digital.Tools.Function.Common
{
    public static class ConfigureDi
    {
        public static ContainerBuilder ConfigureContainerWithCommonModules(RunMode mode)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Properties.Add(nameof(RunMode), mode);
            builder.RegisterModule<CoreAutofacModule>();
            return builder;
        }
    }
}
