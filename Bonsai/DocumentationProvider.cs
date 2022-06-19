using System;
using System.Collections.Generic;
using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.NuGet;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Bonsai
{
    class DocumentationProvider : IDocumentationProvider
    {
        readonly SourceRepository packageRepository;
        readonly PackageConfiguration packageConfiguration;
        readonly IDictionary<string, PackageReference> packageMap;

        public DocumentationProvider(PackageConfiguration configuration, PackageManager packageManager)
        {
            packageConfiguration = configuration;
            packageMap = packageConfiguration.GetPackageReferenceMap();
            packageRepository = packageManager.LocalRepository;
        }

        public string GetDocumentationUrl(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            var packageReference = packageConfiguration.GetAssemblyPackageReference(assemblyName, packageMap);
            if (packageReference == null)
            {
                return null;
            }

            var identity = new PackageIdentity(packageReference.Id, NuGetVersion.Parse(packageReference.Version));
            var package = packageRepository.GetLocalPackage(identity);
            return package?.Nuspec.GetProjectUrl();
        }
    }
}
