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
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var resolver = new PackageAssemblyResolver(configuration, runtimeAssemblies);
            return new MetadataLoadContext(resolver);
        }
    }
}
