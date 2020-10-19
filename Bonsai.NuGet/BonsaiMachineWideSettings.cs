using NuGet.Configuration;
using System;
using System.IO;
using NuGetSettings = global::NuGet.Configuration.Settings;

namespace Bonsai.NuGet
{
    public class BonsaiMachineWideSettings : IMachineWideSettings
    {
        readonly Lazy<ISettings> settings;
        const string SettingsFolderRoot = "Config";

        public BonsaiMachineWideSettings()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            settings = new Lazy<ISettings>(() =>
            {
                var root = Path.Combine(baseDirectory, nameof(NuGet), SettingsFolderRoot);
                return NuGetSettings.LoadMachineWideSettings(root);
            });
        }

        public ISettings Settings => settings.Value;
    }
}
