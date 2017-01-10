using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class BonsaiMachineWideSettings : IMachineWideSettings
    {
        Lazy<IEnumerable<Settings>> settings;
        const string SettingsName = "Bonsai";

        public BonsaiMachineWideSettings()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            settings = new Lazy<IEnumerable<Settings>>(() =>
            {
                return global::NuGet.Settings.LoadMachineWideSettings(
                    new PhysicalFileSystem(baseDirectory),
                    SettingsName);
            });
        }

        public IEnumerable<Settings> Settings
        {
            get { return settings.Value; }
        }
    }
}
