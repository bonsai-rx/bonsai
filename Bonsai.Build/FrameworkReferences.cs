using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.InteropServices;
#endif

namespace Bonsai.Build
{
    /// <summary>
    /// Provides methods for locating the framework assemblies of the running process.
    /// </summary>
    public static class FrameworkReferences
    {
        const string AssemblyExtension = ".dll";

        /// <summary>
        /// Returns the paths of the framework assemblies required to compile code that uses the
        /// specified framework assemblies.
        /// </summary>
        /// <param name="assemblyNames">
        /// The names of the framework assemblies used by the code, with or without the file
        /// extension. Names that are not part of the host framework are ignored.
        /// </param>
        /// <returns>
        /// The paths of the requested assemblies together with the core library and the other
        /// framework assemblies they depend on.
        /// </returns>
        public static IEnumerable<string> GetReferencePaths(IEnumerable<string> assemblyNames)
        {
            if (assemblyNames is null)
                throw new ArgumentNullException(nameof(assemblyNames));

            var names = new HashSet<string>(assemblyNames.Select(GetAssemblyName), StringComparer.OrdinalIgnoreCase);
            return GetHostReferencePaths(names).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        static string GetAssemblyName(string fileName)
        {
            return fileName.EndsWith(AssemblyExtension, StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - AssemblyExtension.Length)
                : fileName;
        }

#if NETFRAMEWORK
        static IEnumerable<string> GetHostReferencePaths(HashSet<string> names)
        {
            var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            yield return Path.Combine(runtimeDirectory, "mscorlib" + AssemblyExtension);
            foreach (var name in names)
            {
                var location = Path.Combine(runtimeDirectory, name + AssemblyExtension);
                if (File.Exists(location))
                    yield return location;
            }
        }
#else
        static IEnumerable<string> GetHostReferencePaths(HashSet<string> names)
        {
            var coreDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var sharedDirectory = Path.GetDirectoryName(Path.GetDirectoryName(coreDirectory));
            var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            var sharedFrameworks = (trustedAssemblies ?? string.Empty)
                .Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(location => location.StartsWith(sharedDirectory, StringComparison.OrdinalIgnoreCase))
                .GroupBy(Path.GetDirectoryName, StringComparer.OrdinalIgnoreCase);

            foreach (var sharedFramework in sharedFrameworks)
            {
                var isCoreFramework = string.Equals(sharedFramework.Key, coreDirectory, StringComparison.OrdinalIgnoreCase);
                if (isCoreFramework || sharedFramework.Any(location => names.Contains(Path.GetFileNameWithoutExtension(location))))
                {
                    foreach (var location in sharedFramework)
                        yield return location;
                }
            }
        }
#endif
    }
}
