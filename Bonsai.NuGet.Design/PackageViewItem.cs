using System.Windows.Forms;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Design
{
    internal class PackageViewItem
    {
        public PackageViewItem(IPackageSearchMetadata package, ImageList imageList, int imageIndex)
        {
            Package = package;
            ImageList = imageList;
            ImageIndex = imageIndex;
        }

        public IPackageSearchMetadata Package { get; }

        public ImageList ImageList { get; }

        public int ImageIndex { get; }
    }
}
