using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Bonsai.Configuration
{
    public class PackageElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public PackageElement this[string name]
        {
            get { return (PackageElement)base.BaseGet(name); }
        }

        protected override string ElementName
        {
            get { return "add"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PackageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var packageElement = element as PackageElement;
            if (packageElement == null)
            {
                throw new ArgumentNullException("element");
            }

            return packageElement.AssemblyName;
        }
    }
}
