using System;
using System.Collections.Generic;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Search
{
    internal static class Repository
    {
        public static ProviderFactory Provider => ProviderFactory.Instance;

        public class ProviderFactory : global::NuGet.Protocol.Core.Types.Repository.ProviderFactory
        {
            internal static readonly ProviderFactory Instance = new();

            public override IEnumerable<Lazy<INuGetResourceProvider>> GetCoreV3()
            {
                yield return new Lazy<INuGetResourceProvider>(() => new PackageTypeSearchResourceV3Provider());
                yield return new Lazy<INuGetResourceProvider>(() => new LocalPackageTypeSearchResourceProvider());
                foreach (var provider in base.GetCoreV3())
                {
                    yield return provider;
                }
            }
        }
    }
}
