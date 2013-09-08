using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    public sealed class AssemblyLocation
    {
        public AssemblyLocation()
        {
        }

        public AssemblyLocation(string name, string path)
        {
            Name = name;
            Path = path;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }
    }
}
