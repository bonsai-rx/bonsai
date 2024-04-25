using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Bonsai.Configuration
{
    public readonly struct ScriptExtensionsProjectMetadata
    {
        private readonly XDocument projectDocument;

        public bool Exists => projectDocument is not null;

        internal const string ItemGroupElement = "ItemGroup";
        internal const string PackageReferenceElement = "PackageReference";
        internal const string PackageIncludeAttribute = "Include";
        internal const string PackageVersionAttribute = "Version";
        internal const string UseWindowsFormsElement = "UseWindowsForms";
        internal const string AllowUnsafeBlocksElement = "AllowUnsafeBlocks";

        internal ScriptExtensionsProjectMetadata(XDocument projectDocument)
            => this.projectDocument = projectDocument;

        private XElement GetProperty(string key)
            => projectDocument?.XPathSelectElement($"/Project/PropertyGroup/{key}");

        private bool GetBoolProperty(string key)
            => String.Equals(GetProperty(key)?.Value, "true", StringComparison.InvariantCultureIgnoreCase);

        public bool AllowUnsafeBlocks => GetBoolProperty(AllowUnsafeBlocksElement);

        public IEnumerable<string> GetAssemblyReferences()
        {
            yield return "System.dll";
            yield return "System.Core.dll";
            yield return "System.Drawing.dll";
            yield return "System.Numerics.dll";
            yield return "System.Reactive.Linq.dll";
            yield return "System.Runtime.Serialization.dll";
            yield return "System.Xml.dll";
            yield return "Bonsai.Core.dll";
            yield return "Microsoft.CSharp.dll";
            yield return "netstandard.dll";

            if (GetBoolProperty(UseWindowsFormsElement))
                yield return "System.Windows.Forms.dll";
        }

        public IEnumerable<string> GetPackageReferences()
        {
            if (!Exists)
                return Enumerable.Empty<string>();

            return from element in projectDocument.Descendants(XName.Get(PackageReferenceElement))
                   let id = element.Attribute(PackageIncludeAttribute)
                   where id != null
                   select id.Value;
        }
    }
}
