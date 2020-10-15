using NuGet.Packaging;
using NuGet.Packaging.Core;
using System.ComponentModel;

namespace Bonsai.NuGet
{
    public class PackageOperationEventArgs : CancelEventArgs
    {
        public PackageOperationEventArgs(PackageIdentity package, PackageReaderBase packageReader, string installPath)
        {
            Package = package;
            PackageReader = packageReader;
            InstallPath = installPath;
        }

        public PackageIdentity Package { get; private set; }

        public PackageReaderBase PackageReader { get; private set; }

        public string InstallPath { get; private set; }
    }
}
