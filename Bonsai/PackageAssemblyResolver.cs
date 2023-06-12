using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bonsai.Configuration;

namespace Bonsai
{
    internal class PackageAssemblyResolver : PathAssemblyResolver
    {
        public PackageAssemblyResolver(PackageConfiguration configuration, IEnumerable<string> assemblyPaths)
            : base(assemblyPaths)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ConfigurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
        }

        private PackageConfiguration Configuration { get; }

        private string ConfigurationRoot { get; }

        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            var assembly = base.Resolve(context, assemblyName);
            if (assembly != null) return assembly;

            var assemblyLocation = Configuration.GetAssemblyLocation(assemblyName.Name);
            if (assemblyLocation != null)
            {
                if (assemblyLocation.StartsWith(Uri.UriSchemeFile) &&
                    Uri.TryCreate(assemblyLocation, UriKind.Absolute, out Uri uri))
                {
                    return context.LoadFromAssemblyPath(uri.LocalPath);
                }

                if (!Path.IsPathRooted(assemblyLocation))
                {
                    assemblyLocation = Path.Combine(ConfigurationRoot, assemblyLocation);
                }

                if (File.Exists(assemblyLocation))
                {
                    return context.LoadFromAssemblyPath(assemblyLocation);
                }
            }

            return null;
        }
    }
}
