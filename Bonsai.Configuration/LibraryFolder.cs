using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    public sealed class LibraryFolder
    {
        public LibraryFolder()
        {
        }

        public LibraryFolder(string path, string platform)
        {
            Path = path;
            Platform = platform;
        }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("platform")]
        public string Platform { get; set; }
    }
}
