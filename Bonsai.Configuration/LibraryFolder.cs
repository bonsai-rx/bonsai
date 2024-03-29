﻿using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    [DebuggerDisplay("{Path} ({Platform,nq})")]
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
