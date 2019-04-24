using Microsoft.Extensions.Configuration;
using System;
using IConfigurationProvider = DFC.Digital.Tools.Data.Interfaces.IConfigurationProvider;

namespace DFC.Digital.Tools.Core
{
    public class AppConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration configuration;

        public AppConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Add<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public T GetConfig<T>(string key)
        {
            var value = this.configuration[key];
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public T GetConfig<T>(string key, T defaultValue)
        {
            var value = this.configuration[key];
            return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
