using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    [DebuggerDisplay("{AssemblyName,nq} ({ProcessorArchitecture,nq}): {Location}")]
    public sealed class AssemblyLocation
    {
        public AssemblyLocation()
        {
        }

        public AssemblyLocation(string assemblyName, ProcessorArchitecture processorArchitecture, string path)
        {
            AssemblyName = assemblyName;
            ProcessorArchitecture = processorArchitecture;
            Location = path;
        }

        [XmlAttribute("assemblyName")]
        public string AssemblyName { get; set; }

        [XmlAttribute("processorArchitecture")]
        public ProcessorArchitecture ProcessorArchitecture { get; set; }

        [XmlAttribute("location")]
        public string Location { get; set; }
    }
}
