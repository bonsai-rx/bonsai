using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    [Serializable]
    public class PackageConfiguration
    {
        readonly PackageReferenceCollection packages = new PackageReferenceCollection();
        readonly AssemblyReferenceCollection assemblyReferences = new AssemblyReferenceCollection();
        readonly AssemblyLocationCollection assemblyLocations = new AssemblyLocationCollection();
        readonly LibraryFolderCollection libraryFolders = new LibraryFolderCollection();

        [XmlIgnore]
        public string ConfigurationFile { get; internal set; }

        [XmlArrayItem("Package")]
        public PackageReferenceCollection Packages
        {
            get { return packages; }
        }

        public AssemblyReferenceCollection AssemblyReferences
        {
            get { return assemblyReferences; }
        }

        public AssemblyLocationCollection AssemblyLocations
        {
            get { return assemblyLocations; }
        }

        public LibraryFolderCollection LibraryFolders
        {
            get { return libraryFolders; }
        }
    }
}
