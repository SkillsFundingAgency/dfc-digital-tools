using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IConfigConfigurationProvider
    {
        T GetConfig<T>(string key);

        T GetConfig<T>(string key, T defaultValue);

        T GetConfigSectionKey<T>(string section, string key);

        void Add<T>(string key, T value);
    }
}
