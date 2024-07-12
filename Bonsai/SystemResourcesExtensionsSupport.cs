using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Bonsai
{
    /// <summary>Enables support for System.Resources.Extensions when the bootstrapper is ILRepacked</summary>
    /// <remarks>
    /// System.Resources.Extensions cannot be cleanly repacked because resources using modern resource embedding internally refer to it
    /// using an explicit strong assembly name. As such the runtime will refuse to consider our own assembly a valid provider for it.
    ///
    /// In order to work around this, we embed it (and its dependencies) directly and load them in an assembly resolver instead.
    /// </remarks>
    internal static class SystemResourcesExtensionsSupport
    {
        [Conditional("NETFRAMEWORK")]
        internal static void Initialize()
        {
            // Assembly.Load(byte[]) always results in a new assembly so we need to manually de-duplicate them
            ConcurrentDictionary<string, Assembly> resolveCache = new();

            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);

                if (resolveCache.TryGetValue(assemblyName.Name, out var cachedResult))
                    return cachedResult;

                using var embeddedAssembly = typeof(SystemResourcesExtensionsSupport).Assembly.GetManifestResourceStream($"{nameof(Bonsai)}.{assemblyName.Name}.dll");

                if (embeddedAssembly is null)
                    return resolveCache.GetOrAdd(assemblyName.Name, (Assembly)null);

                var assemblyBytes = new byte[embeddedAssembly.Length];
                int readLength = embeddedAssembly.Read(assemblyBytes, 0, assemblyBytes.Length);
                Debug.Assert(readLength == assemblyBytes.Length);

                var result = Assembly.Load(assemblyBytes);
                result = resolveCache.GetOrAdd(assemblyName.Name, result);
                Debug.WriteLine($"Redirecting '{args.Name}' to embedded '{result.FullName}'");
                return result;
            };
        }
    }
}
