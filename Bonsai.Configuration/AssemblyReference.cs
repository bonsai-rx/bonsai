using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    public sealed class AssemblyReference
    {
        public AssemblyReference()
        {
        }

        public AssemblyReference(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        [XmlAttribute("assemblyName")]
        public string AssemblyName { get; set; }
    }
}
