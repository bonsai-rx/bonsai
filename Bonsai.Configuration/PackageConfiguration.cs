using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Bonsai.Configuration
{
    public class PackageConfiguration : ConfigurationSection
    {
        public const string SectionName = "packages";
        static readonly ConfigurationProperty PackagesProperty = new ConfigurationProperty(null, typeof(PackageElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        static readonly ConfigurationPropertyCollection ConfigurationProperties = new ConfigurationPropertyCollection { PackagesProperty };

        protected override ConfigurationPropertyCollection Properties
        {
            get { return ConfigurationProperties; }
        }

        public PackageElementCollection Packages
        {
            get { return this[PackagesProperty] as PackageElementCollection; }
        }
    }
}
