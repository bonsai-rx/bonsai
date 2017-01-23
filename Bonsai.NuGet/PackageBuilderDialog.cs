using Bonsai.Design;
using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.NuGet
{
    public partial class PackageBuilderDialog : Form
    {
        bool splitterMoving;
        string metadataPath;
        int metadataVersion;
        int metadataSaveVersion;
        PackageBuilder packageBuilder;
        PhysicalPackageFile entryPoint;
        PhysicalPackageFile entryPointLayout;
        static readonly PackageBuilderTypeDescriptionProvider descriptionProvider = new PackageBuilderTypeDescriptionProvider();

        public PackageBuilderDialog()
        {
            InitializeComponent();
            metadataProperties.PropertyValueChanged += (sender, e) => UpdateMetadataVersion();
        }

        public string MetadataPath
        {
            get { return metadataPath; }
            set { metadataPath = value; }
        }

        public string InitialDirectory
        {
            get { return saveFileDialog.InitialDirectory; }
            set { saveFileDialog.InitialDirectory = value; }
        }

        bool MetadataSpecified
        {
            get { return !string.IsNullOrEmpty(metadataPath); }
        }

        bool SaveMetadata()
        {
            if (!MetadataSpecified)
            {
                throw new InvalidOperationException("No valid metadata path was specified.");
            }

            var metadataExists = metadataSaveVersion >= 0;
            if (metadataExists ||
                MessageBox.Show(this,
                                Resources.CreatePackageMetadata, Text,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var manifest = Manifest.Create(packageBuilder);
                if (metadataExists)
                {
                    using (var stream = File.OpenRead(metadataPath))
                    {
                        var existingManifest = Manifest.ReadFrom(stream, true);
                        if (existingManifest.Files != null)
                        {
                            manifest.Files = existingManifest.Files;
                        }
                    }
                }

                using (var stream = File.Open(metadataPath, FileMode.Create))
                {
                    manifest.Save(stream);
                    metadataSaveVersion = metadataVersion;
                    return true;
                }
            }
            else return false;
        }

        static void EnsureDirectory(string path)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(path);
        }

        static void RenamePackageFile(PhysicalPackageFile file, string fileName)
        {
            var extension = Path.GetExtension(file.SourcePath);
            if (extension == Constants.LayoutExtension) extension = Constants.BonsaiExtension + extension;
            var basePath = Path.GetDirectoryName(file.TargetPath);
            file.TargetPath = Path.Combine(basePath, fileName + extension);
        }

        void AddPackageFile(IPackageFile file)
        {
            var nodes = contentView.Nodes;
            var pathElements = file.Path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            for (int i = 0; i < pathElements.Length; i++)
            {
                var node = nodes[pathElements[i]];
                if (node == null)
                {
                    node = nodes.Add(pathElements[i], pathElements[i]);
                }

                nodes = node.Nodes;
            }
        }

        public void UpdateMetadataVersion()
        {
            metadataVersion++;
        }

        public void SetPackageBuilder(PackageBuilder builder)
        {
            SuspendLayout();
            packageBuilder = builder;
            metadataSaveVersion = MetadataSpecified && File.Exists(metadataPath) ? 0 : -1;
            TypeDescriptor.AddProvider(descriptionProvider, packageBuilder);
            metadataProperties.SelectedObject = packageBuilder;
            metadataProperties.ExpandAllGridItems();
            var entryPointPath = Path.GetFileNameWithoutExtension(metadataPath) + Constants.BonsaiExtension;
            var entryPointLayoutPath = entryPointPath + Constants.LayoutExtension;
            foreach (var file in packageBuilder.Files)
            {
                if (file.EffectivePath == entryPointPath) entryPoint = file as PhysicalPackageFile;
                if (file.EffectivePath == entryPointLayoutPath) entryPointLayout = file as PhysicalPackageFile;
                AddPackageFile(file);
            }
            contentView.ExpandAll();
            ResumeLayout();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            const int HighDpiGridOffset = 13;
            if (factor.Height > 1.5f)
            {
                contentView.SuspendLayout();
                contentView.Top -= HighDpiGridOffset;
                contentView.Height += HighDpiGridOffset;
                contentView.ResumeLayout();
            }
            base.ScaleControl(factor, specified);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (MetadataSpecified && metadataVersion > metadataSaveVersion)
            {
                var result = MessageBox.Show(
                    this, Resources.SavePackageMetadata, Text,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel ||
                    result == DialogResult.Yes && !SaveMetadata())
                {
                    DialogResult = DialogResult.None;
                    e.Cancel = true;
                }
            }

            if (DialogResult == DialogResult.OK)
            {
                var packageFileName =
                    packageBuilder.Id + "." +
                    packageBuilder.Version + global::NuGet.Constants.PackageExtension;
                saveFileDialog.FileName = packageFileName;
                if (entryPoint != null)
                {
                    RenamePackageFile(entryPoint, packageBuilder.Id);
                    if (entryPointLayout != null) RenamePackageFile(entryPointLayout, packageBuilder.Id);
                }

                EnsureDirectory(saveFileDialog.InitialDirectory);
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var dialog = new PackageOperationDialog())
                    {
                        ILogger logger = new EventLogger();
                        dialog.Text = Resources.ExportOperationLabel;
                        dialog.RegisterEventLogger((EventLogger)logger);
                        logger.Log(MessageLevel.Info,
                                   "Creating package '{0} {1}'.",
                                   packageBuilder.Id,
                                   packageBuilder.Version);
                        var dialogClosed = Observable.FromEventPattern<FormClosedEventHandler, FormClosedEventArgs>(
                            handler => dialog.FormClosed += handler,
                            handler => dialog.FormClosed -= handler);
                        var operation = Observable.Using(
                            () => Stream.Synchronized(File.Open(saveFileDialog.FileName, FileMode.Create)),
                            stream => Observable.Start(() => packageBuilder.Save(stream)).TakeUntil(dialogClosed));
                        using (var subscription = operation.ObserveOn(this).Subscribe(
                            xs => dialog.Complete(),
                            ex => logger.Log(MessageLevel.Error, ex.Message)))
                        {
                            if (dialog.ShowDialog() != DialogResult.OK)
                            {
                                e.Cancel = true;
                            }
                            else
                            {
                                SystemSounds.Asterisk.Play();
                                var message = string.Format(Resources.PackageExported, packageBuilder.Id, packageBuilder.Version);
                                MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.None);
                            }
                        }
                    }
                }
                else e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            splitterMoving = e.X > metadataProperties.Right && e.X < contentView.Left;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var min = metadataProperties.Right;
            var max = contentView.Left;
            if (splitterMoving || e.X > min && e.X < max)
            {
                Cursor = Cursors.VSplit;
            }
            else Cursor = Cursors.Default;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (splitterMoving)
            {
                var splitterPos = (metadataProperties.Right + contentView.Left) / 2;
                var min = metadataProperties.Left + metadataLabel.Width;
                var max = contentView.Right - contentLabel.Width;
                var x = Math.Max(min, Math.Min(e.X, max));
                var offset = x - splitterPos;

                metadataProperties.Width += offset;
                contentView.SuspendLayout();
                contentLabel.Left += offset;
                contentView.Left += offset;
                contentView.Width -= offset;
                contentView.ResumeLayout();
            }
            splitterMoving = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!splitterMoving) Cursor = Cursors.Default;
            base.OnMouseLeave(e);
        }

        class PackageBuilderTypeDescriptionProvider : TypeDescriptionProvider
        {
            readonly PackageBuilderTypeDescriptor typeDescriptor = new PackageBuilderTypeDescriptor();

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
                    "Description",
                    "Authors",

                    "Title",
                    "Owners",
                    "ProjectUrl",
                    "LicenseUrl",
                    "IconUrl",
                    "RequireLicenseAcceptance",
                    "Summary",
                    "ReleaseNotes",
                    "Copyright",
                    "Tags",
                    "DependencySets",
                };

                static readonly Dictionary<string, string> DescriptionMap = new Dictionary<string, string>
                {
                    { "Id", "The case-insensitive package identifier, which must be unique across the package gallery. IDs may not contain spaces or characters that are not valid for a URL." },
                    { "Version", "The version of the package, following the major.minor.patch pattern. Version numbers may include a pre-release suffix." },
                    { "Description", "A long description of the package for UI display." },
                    { "Authors", "A comma-separated list of package authors, matching the profile names on nuget.org." },
                    { "Title", "A human-friendly title of the package. If not specified, the package ID is used instead." },
                    { "Owners", "A comma-separated list of the package creators using profile names on nuget.org. This is often the same list as in authors." },
                    { "ProjectUrl", "A URL for the package's home page, often shown in UI displays as well as nuget.org." },
                    { "LicenseUrl", "A URL for the package's license, often shown in UI displays as well as nuget.org." },
                    { "IconUrl", "A URL for a 64x64 image with transparent background to use as the icon for the package in UI display." },
                    { "RequireLicenseAcceptance", "A value specifying whether the client must prompt the consumer to accept the package license before installing the package." },
                    { "Summary", "A short description of the package for UI display. If omitted, a truncated version of the description is used." },
                    { "ReleaseNotes", "A description of the changes made in this release of the package, often used in the Updates tab in place of the package description. " },
                    { "Copyright", "Copyright details for the package." },
                    { "Tags", "A space-delimited list of tags and keywords that describe the package and aid discoverability of packages through search and filtering mechanisms." },
                    { "DependencySets", "The collection of dependencies for the package." }
                };

                DescriptionAttribute GetDescriptionAttribute(PropertyDescriptor descriptor)
                {
                    string description;
                    if (DescriptionMap.TryGetValue(descriptor.Name, out description))
                    {
                        return new DescriptionAttribute(description);
                    }

                    return DescriptionAttribute.Default;
                }

                PropertyDescriptor ConvertPropertyDescriptor(PropertyDescriptor descriptor)
                {
                    var descriptionAttribute = GetDescriptionAttribute(descriptor);
                    if (descriptor.Name == "DependencySets")
                    {
                        var typeConverterAttribute = new TypeConverterAttribute(typeof(DependencySetConverter));
                        var attributes = new Attribute[] { descriptionAttribute, typeConverterAttribute };
                        return new SimplePropertyDescriptor(descriptor, "Dependencies", attributes);
                    }

                    if (descriptor.Name == "Authors" || descriptor.Name == "Owners")
                    {
                        var typeConverterAttribute = new TypeConverterAttribute(typeof(CommaDelimitedSetConverter));
                        var attributes = new Attribute[] { descriptionAttribute, typeConverterAttribute };
                        return new SetPropertyDescriptor(descriptor, attributes);
                    }

                    if (descriptor.Name == "Tags")
                    {
                        var typeConverterAttribute = new TypeConverterAttribute(typeof(TagSetConverter));
                        var attributes = new Attribute[] { descriptionAttribute, typeConverterAttribute };
                        return new SetPropertyDescriptor(descriptor, attributes);
                    }

                    if (descriptor.Name == "Description" || descriptor.Name == "Summary" || descriptor.Name == "ReleaseNotes")
                    {
                        var editorAttribute = new EditorAttribute(DesignTypes.MultilineStringEditor, typeof(UITypeEditor));
                        return new SimplePropertyDescriptor(descriptor, new Attribute[] { descriptionAttribute, editorAttribute });
                    }

                    return new SimplePropertyDescriptor(descriptor, new Attribute[] { descriptionAttribute });
                }

                public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
                {
                    var properties = from property in base.GetProperties(attributes).Cast<PropertyDescriptor>()
                                     where property.Name != "Files" &&
                                           property.Name != "Language" &&
                                           property.Name != "MinClientVersion" &&
                                           property.Name != "ContentFiles" &&
                                           property.Name != "DevelopmentDependency" &&
                                           property.Name != "FrameworkReferences" &&
                                           property.Name != "PackageAssemblyReferences"
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

                class ConstantPropertyDescriptor : PropertyDescriptor
                {
                    readonly object constant;

                    public ConstantPropertyDescriptor(string name, object value)
                        : base(name, new Attribute[0])
                    {
                        if (value == null)
                        {
                            throw new ArgumentNullException("value");
                        }

                        constant = value;
                    }

                    public override bool CanResetValue(object component)
                    {
                        return false;
                    }

                    public override Type ComponentType
                    {
                        get { return null; }
                    }

                    public override object GetValue(object component)
                    {
                        return constant;
                    }

                    public override bool IsReadOnly
                    {
                        get { return true; }
                    }

                    public override Type PropertyType
                    {
                        get { return constant.GetType(); }
                    }

                    public override void ResetValue(object component)
                    {
                        throw new NotSupportedException();
                    }

                    public override void SetValue(object component, object value)
                    {
                        throw new NotSupportedException();
                    }

                    public override bool ShouldSerializeValue(object component)
                    {
                        return false;
                    }
                }

                class CommaDelimitedSetConverter : TypeConverter
                {
                    public virtual string SetSeparator
                    {
                        get { return ","; }
                    }

                    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                    {
                        var set = value as IEnumerable<string>;
                        if (set != null)
                        {
                            return string.Join(SetSeparator, set);
                        }

                        return base.ConvertTo(context, culture, value, destinationType);
                    }

                    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                    {
                        if (sourceType == typeof(string)) return true;
                        return base.CanConvertFrom(context, sourceType);
                    }

                    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                    {
                        var text = value as string;
                        if (text != null)
                        {
                            var names = text.Split(new[] { SetSeparator }, StringSplitOptions.RemoveEmptyEntries);
                            var set = context.PropertyDescriptor.GetValue(context.Instance) as ISet<string>;
                            if (set != null)
                            {
                                set.Clear();
                                foreach (var name in names)
                                {
                                    set.Add(name);
                                }
                            }

                            return set;
                        }

                        return base.ConvertFrom(context, culture, value);
                    }
                }

                class TagSetConverter : CommaDelimitedSetConverter
                {
                    public override string SetSeparator
                    {
                        get { return " "; }
                    }

                    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                    {
                        var set = value as IEnumerable<string>;
                        if (set != null)
                        {
                            value = set.Where(tag => tag != Constants.BonsaiDirectory && tag != Constants.GalleryDirectory);
                        }

                        return base.ConvertTo(context, culture, value, destinationType);
                    }

                    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                    {
                        var result = base.ConvertFrom(context, culture, value);
                        var set = result as ISet<string>;
                        if (set != null)
                        {
                            set.Add(Constants.BonsaiDirectory);
                            set.Add(Constants.GalleryDirectory);
                        }

                        return result;
                    }
                }

                class DependencySetConverter : TypeConverter
                {
                    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                    {
                        if (destinationType == typeof(string))
                        {
                            return "(Collection)";
                        }

                        return base.ConvertTo(context, culture, value, destinationType);
                    }

                    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
                    {
                        return true;
                    }

                    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
                    {
                        var dependencySet = value as Collection<PackageDependencySet>;
                        if (dependencySet != null)
                        {
                            var properties = from set in dependencySet
                                             from dependency in set.Dependencies
                                             select new ConstantPropertyDescriptor(dependency.Id, dependency.VersionSpec);
                            return new PropertyDescriptorCollection(properties.ToArray());
                        }

                        return base.GetProperties(context, value, attributes);
                    }
                }
            }
        }
    }
}
