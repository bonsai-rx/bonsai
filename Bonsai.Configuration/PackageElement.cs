using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Bonsai.Configuration
{
    public class PackageElement : ConfigurationElement
    {
        static readonly ConfigurationProperty AssemblyNameProperty = new ConfigurationProperty("assembly", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        static readonly ConfigurationProperty AssemblyLocationProperty = new ConfigurationProperty("location", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        static readonly ConfigurationProperty DependencyProperty = new ConfigurationProperty("dependency", typeof(bool), false, ConfigurationPropertyOptions.None);
        static readonly ConfigurationPropertyCollection ConfigurationProperties = new ConfigurationPropertyCollection { AssemblyNameProperty, AssemblyLocationProperty, DependencyProperty };

        public PackageElement()
        {
        }

        public PackageElement(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return ConfigurationProperties; }
        }

        public string AssemblyName
        {
            get { return this[AssemblyNameProperty] as string; }
            set { this[AssemblyNameProperty] = value; }
        }

        public string AssemblyLocation
        {
            get { return this[AssemblyLocationProperty] as string; }
            set { this[AssemblyLocationProperty] = value; }
        }

        public bool Dependency
        {
            get { return (bool)this[DependencyProperty]; }
            set { this[DependencyProperty] = value; }
        }
    }
}
