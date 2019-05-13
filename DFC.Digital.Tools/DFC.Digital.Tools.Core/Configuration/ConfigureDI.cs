using Autofac;

namespace DFC.Digital.Tools.Core
{
    public static class ConfigureDI
    {
        /// <summary>
        /// Configure DI container builder for this module
        /// </summary>
        /// <param name="mode">Set depending on where we are running Azure or Console</param>
        /// <returns>The Container Builder</returns>
        public static ContainerBuilder ConfigureContainerWithCommonModules(RunMode mode)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Properties.Add(nameof(RunMode), mode);
            builder.RegisterModule<CoreAutofacModule>();
            return builder;
        }
    }
}
