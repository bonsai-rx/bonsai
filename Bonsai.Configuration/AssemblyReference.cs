using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    [DebuggerDisplay("{AssemblyName}")]
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
