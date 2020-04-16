using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    [DebuggerDisplay("\"{Id,nq} ({Version,nq})\"")]
    public sealed class PackageReference
    {
        public PackageReference()
        {
        }

        public PackageReference(string id, string version)
        {
            Id = id;
            Version = version;
        }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }
    }
}
