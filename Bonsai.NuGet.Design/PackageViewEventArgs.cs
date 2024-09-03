using System;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Design
{
    internal delegate void PackageViewEventHandler(object sender, PackageViewEventArgs e);

    internal class PackageViewEventArgs : EventArgs
    {
        public PackageViewEventArgs(IPackageSearchMetadata package, PackageOperationType operation)
        {
            Package = package;
            Operation = operation;
        }

        public IPackageSearchMetadata Package { get; }

        public PackageOperationType Operation { get; }
    }
}
