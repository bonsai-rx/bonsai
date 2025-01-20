using System;

namespace Bonsai.NuGet.Design
{
    internal delegate void PackageSearchEventHandler(object sender, PackageSearchEventArgs e);

    internal class PackageSearchEventArgs : EventArgs
    {
        public PackageSearchEventArgs(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        public string SearchTerm { get; }
    }
}
