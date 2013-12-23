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

        public BonsaiMachineWideSettings()
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            settings = new Lazy<IEnumerable<Settings>>(() =>
            {
                return global::NuGet.Settings.LoadMachineWideSettings(
                    new PhysicalFileSystem(baseDirectory),
                    "Bonsai");
            });
        }

        public IEnumerable<Settings> Settings
        {
            get { return settings.Value; }
        }
    }
}
