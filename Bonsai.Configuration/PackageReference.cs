using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    public sealed class PackageReference
    {
        public PackageReference()
        {
        }

        public PackageReference(string name)
        {
            Name = name;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
