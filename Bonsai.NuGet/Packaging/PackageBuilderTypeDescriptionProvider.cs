using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NuGet.Packaging;

namespace Bonsai.NuGet.Packaging
{
    public class PackageBuilderTypeDescriptionProvider : TypeDescriptionProvider
    {
        const string RequiredCategory = "\t\t\tRequired";
        const string LicenseCategory = "\t\tLicense";
        const string AboutCategory = "\tAbout";
        readonly PackageBuilderTypeDescriptor typeDescriptor = new();

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return typeDescriptor;
        }

        class PackageBuilderTypeDescriptor : CustomTypeDescriptor
        {
            static readonly ICustomTypeDescriptor baseDescriptor = TypeDescriptor.GetProvider(typeof(PackageBuilder))
                                                                                 .GetTypeDescriptor(typeof(PackageBuilder));

            public PackageBuilderTypeDescriptor()
                : base(baseDescriptor)
            {
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                var properties = base.GetProperties();
                return properties;
            }

            static readonly string[] SortOrder = new[]
            {
                    "Id",
                    "Version",
                    "Authors",
                    "Description",
                    "Tags",

                    "License",
                    "RequireLicenseAcceptance",

                    "ProjectUrl",
                    "Copyright",
                    "Readme",
                    "Icon",

                    "DependencySets",
                };

            static readonly Dictionary<string, string> DescriptionMap = new Dictionary<string, string>
                {
                    { "Id", "The case-insensitive package identifier, which must be unique across the package gallery. IDs may not contain spaces or characters that are not valid for a URL." },
                    { "Version", "The version of the package, following the major.minor.patch pattern. Version numbers may include a pre-release suffix." },
                    { "Authors", "A comma-separated list of package authors, matching the profile names on nuget.org." },
                    { "Description", "A long description of the package for UI display." },
                    { "Tags", "A space-delimited list of tags and keywords that describe the package and aid discoverability of packages through search and filtering mechanisms." },
                    { "LicenseMetadata", "The SPDX license expression or path to a license file within the package, often shown in UI displays as well as nuget.org." },
                    { "RequireLicenseAcceptance", "A value specifying whether the client must prompt the consumer to accept the package license before installing the package." },
                    { "ProjectUrl", "A URL for the package's home page, often shown in UI displays as well as nuget.org." },
                    { "Copyright", "Copyright details for the package." },
                    { "Readme", "The path to the package README file, relative to the root of the project." },
                    { "Icon", "A path to an image file within the package to be used as the package icon." },
                    { "DependencyGroups", "The collection of dependencies for the package." }
                };

            static readonly Dictionary<string, string> CategoryMap = new Dictionary<string, string>
                {
                    { "Id", RequiredCategory },
                    { "Version", RequiredCategory },
                    { "Authors", RequiredCategory },
                    { "Description", RequiredCategory },
                    { "Tags", RequiredCategory },
                    { "LicenseMetadata", LicenseCategory },
                    { "RequireLicenseAcceptance", LicenseCategory },
                    { "ProjectUrl", AboutCategory },
                    { "Copyright", AboutCategory },
                    { "Readme", AboutCategory },
                    { "Icon", AboutCategory },
                    { "DependencyGroups", default }
                };

            static DescriptionAttribute GetDescriptionAttribute(PropertyDescriptor descriptor)
            {
                if (DescriptionMap.TryGetValue(descriptor.Name, out string description))
                {
                    return new DescriptionAttribute(description);
                }

                return DescriptionAttribute.Default;
            }

            static CategoryAttribute GetCategoryAttribute(PropertyDescriptor descriptor)
            {
                if (CategoryMap.TryGetValue(descriptor.Name, out string category))
                {
                    return new CategoryAttribute(category);
                }

                return CategoryAttribute.Default;
            }

            static PropertyDescriptor ConvertPropertyDescriptor(PropertyDescriptor descriptor)
            {
                var descriptionAttribute = GetDescriptionAttribute(descriptor);
                var categoryAttribute = GetCategoryAttribute(descriptor);
                if (descriptor.Name == "DependencyGroups")
                {
                    var typeConverterAttribute = new TypeConverterAttribute(typeof(DependencySetConverter));
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, typeConverterAttribute };
                    return new SimplePropertyDescriptor(descriptor, "Dependencies", attributes);
                }

                if (descriptor.Name == "Authors" || descriptor.Name == "Owners")
                {
                    var typeConverterAttribute = new TypeConverterAttribute(typeof(CommaDelimitedSetConverter));
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, typeConverterAttribute };
                    return new SetPropertyDescriptor(descriptor, attributes);
                }

                if (descriptor.Name == "Version")
                {
                    var typeConverterAttribute = new TypeConverterAttribute(typeof(NuGetVersionConverter));
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, typeConverterAttribute };
                    return new SimplePropertyDescriptor(descriptor, attributes);
                }

                if (descriptor.Name == "Tags")
                {
                    var typeConverterAttribute = new TypeConverterAttribute(typeof(TagSetConverter));
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, typeConverterAttribute };
                    return new SetPropertyDescriptor(descriptor, attributes);
                }

                if (descriptor.Name == "Description")
                {
                    var editorAttribute = new EditorAttribute(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor);
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, editorAttribute };
                    return new NonEmptyPropertyDescriptor(descriptor, attributes);
                }

                if (descriptor.Name == "LicenseMetadata")
                {
                    var typeConverterAttribute = new TypeConverterAttribute(typeof(LicenseMetadataConverter));
                    var editorAttribute = new EditorAttribute(
                        "Bonsai.NuGet.Design.LicenseMetadataEditor, Bonsai.NuGet.Design",
                        DesignTypes.UITypeEditor);
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, typeConverterAttribute, editorAttribute };
                    return new SimplePropertyDescriptor(descriptor, "License", attributes);
                }

                if (descriptor.Name == "Readme" || descriptor.Name == "Icon")
                {
                    var editorAttribute = new EditorAttribute(DesignTypes.OpenFileNameEditor, DesignTypes.UITypeEditor);
                    var attributes = new Attribute[] { descriptionAttribute, categoryAttribute, editorAttribute };
                    return new NonEmptyPropertyDescriptor(descriptor, attributes);
                }

                return new SimplePropertyDescriptor(descriptor, new Attribute[] { descriptionAttribute, categoryAttribute });
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = from property in base.GetProperties(attributes).Cast<PropertyDescriptor>()
                                 where property.Name != nameof(PackageBuilder.Title) &&
                                       property.Name != nameof(PackageBuilder.Owners) &&
                                       property.Name != nameof(PackageBuilder.IconUrl) &&
                                       property.Name != nameof(PackageBuilder.LicenseUrl) &&
                                       property.Name != nameof(PackageBuilder.Summary) &&
                                       property.Name != nameof(PackageBuilder.ReleaseNotes) &&
                                       property.Name != nameof(PackageBuilder.Files) &&
                                       property.Name != nameof(PackageBuilder.Language) &&
                                       property.Name != nameof(PackageBuilder.MinClientVersion) &&
                                       property.Name != nameof(PackageBuilder.ContentFiles) &&
                                       property.Name != nameof(PackageBuilder.DevelopmentDependency) &&
                                       property.Name != nameof(PackageBuilder.FrameworkReferences) &&
                                       property.Name != nameof(PackageBuilder.FrameworkReferenceGroups) &&
                                       property.Name != nameof(PackageBuilder.PackageAssemblyReferences) &&
                                       property.Name != nameof(PackageBuilder.HasSnapshotVersion) &&
                                       property.Name != nameof(PackageBuilder.OutputName) &&
                                       property.Name != nameof(PackageBuilder.PackageTypes) &&
                                       property.Name != nameof(PackageBuilder.Properties) &&
                                       property.Name != nameof(PackageBuilder.Repository) &&
                                       property.Name != nameof(PackageBuilder.Serviceable) &&
                                       property.Name != nameof(PackageBuilder.TargetFrameworks) &&
                                       property.Name != nameof(PackageBuilder.EmitRequireLicenseAcceptance)
                                 select ConvertPropertyDescriptor(property);
                var output = new PropertyDescriptorCollection(properties.ToArray()).Sort(SortOrder);
                return output;
            }

            class SimplePropertyDescriptor : PropertyDescriptor
            {
                readonly PropertyDescriptor descriptor;

                public SimplePropertyDescriptor(PropertyDescriptor descr, Attribute[] attrs)
                    : base(descr, attrs)
                {
                    descriptor = descr;
                }

                public SimplePropertyDescriptor(PropertyDescriptor descr, string name, Attribute[] attrs)
                    : base(name, attrs)
                {
                    descriptor = descr;
                }

                protected PropertyDescriptor Descriptor
                {
                    get { return descriptor; }
                }

                public override bool CanResetValue(object component)
                {
                    return descriptor.CanResetValue(component);
                }

                public override Type ComponentType
                {
                    get { return descriptor.ComponentType; }
                }

                public override object GetValue(object component)
                {
                    return descriptor.GetValue(component);
                }

                public override bool IsReadOnly
                {
                    get { return descriptor.IsReadOnly; }
                }

                public override Type PropertyType
                {
                    get { return descriptor.PropertyType; }
                }

                public override void ResetValue(object component)
                {
                    descriptor.ResetValue(component);
                }

                public override void SetValue(object component, object value)
                {
                    descriptor.SetValue(component, value);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return descriptor.ShouldSerializeValue(component);
                }
            }

            class NonEmptyPropertyDescriptor : SimplePropertyDescriptor
            {
                public NonEmptyPropertyDescriptor(PropertyDescriptor descr, Attribute[] attrs)
                    : base(descr, attrs)
                {
                }

                public override void SetValue(object component, object value)
                {
                    if (value is string text && string.IsNullOrEmpty(text))
                        value = null;

                    base.SetValue(component, value);
                }
            }

            class SetPropertyDescriptor : SimplePropertyDescriptor
            {
                public SetPropertyDescriptor(PropertyDescriptor descr, Attribute[] attrs)
                    : base(descr, attrs)
                {
                }

                public override bool IsReadOnly
                {
                    get { return false; }
                }

                public override void SetValue(object component, object value)
                {
                    var set = GetValue(component);
                    if (set != value)
                    {
                        throw new InvalidOperationException("Attempted to set a virtual read-only property");
                    }
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return true;
                }
            }
        }
    }
}
