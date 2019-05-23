using Autofac;
using Microsoft.Extensions.Configuration;

namespace DFC.Digital.Tools.Core
{
    public static class ConfigureDI
    {
        /// <summary>
        /// Configure DI container builder for this module
        /// </summary>
        /// <param name="mode">Set depending on where we are running Azure or Console</param>
        /// <param name="basePath">Path to the settings file</param>
        /// <returns>The Container Builder</returns>
        public static ContainerBuilder ConfigureContainerWithCommonModules(RunMode mode, string basePath)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Properties.Add(nameof(RunMode), mode);
            builder.RegisterModule<CoreAutofacModule>();
            builder.Register(c => new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build())
                .As<IConfiguration>();

            return builder;
        }
    }
}
