using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    public sealed class LibraryFolder
    {
        public LibraryFolder()
        {
        }

        public LibraryFolder(string path)
        {
            Path = path;
        }

        [XmlAttribute("path")]
        public string Path { get; set; }
    }
}
