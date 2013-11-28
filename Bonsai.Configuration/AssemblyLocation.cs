using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    public sealed class AssemblyLocation
    {
        public AssemblyLocation()
        {
        }

        public AssemblyLocation(string assemblyName, string path)
        {
            AssemblyName = assemblyName;
            Location = path;
        }

        [XmlAttribute("assemblyName")]
        public string AssemblyName { get; set; }

        [XmlAttribute("location")]
        public string Location { get; set; }
    }
}
