using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace Bonsai.NuGet.Packaging
{
    public class BonsaiMetadataPackageFile : IPackageFile
    {
        static readonly DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        public BonsaiMetadataPackageFile(BonsaiMetadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public BonsaiMetadata Metadata { get; }

        public string Path => Constants.BonsaiMetadataFile;

        public string EffectivePath => Path;

        public FrameworkName TargetFramework => null;

        public NuGetFramework NuGetFramework => null;

        public DateTimeOffset LastWriteTime { get; private set; }

        public Stream GetStream()
        {
            LastWriteTime = DateTimeOffset.UtcNow;
            var streamContents = JsonConvert.SerializeObject(Metadata, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            return new MemoryStream(Encoding.UTF8.GetBytes(streamContents));
        }
    }
}
