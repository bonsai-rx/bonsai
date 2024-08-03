using Bonsai.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Bonsai
{
    static class LoaderResource
    {
        public static MetadataLoadContext CreateMetadataLoadContext(PackageConfiguration configuration)
        {
            var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            var runtimeAssemblies = Directory.EnumerateFiles(runtimeDirectory, "*.dll");
#if NETCOREAPP
            var windowsDesktopDirectory = Path.GetDirectoryName(typeof(System.Windows.Forms.Form).Assembly.Location);
            var windowsDesktopAssemblies = Directory.EnumerateFiles(windowsDesktopDirectory, "*.dll");
            runtimeAssemblies = System.Linq.Enumerable.Concat(runtimeAssemblies, windowsDesktopAssemblies);
#endif
            var resolver = new PackageAssemblyResolver(configuration, runtimeAssemblies);
            return new MetadataLoadContext(resolver);
        }
    }
}
