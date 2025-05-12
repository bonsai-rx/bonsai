using Bonsai.Design;
using Bonsai.NuGet.Design.Properties;
using Bonsai.NuGet.Packaging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public partial class GalleryPackageBuilderDialog : Form
    {
        bool splitterMoving;
        string metadataPath;
        int metadataVersion;
        int metadataSaveVersion;
        PackageBuilder packageBuilder;
        PhysicalPackageFile entryPoint;
        PhysicalPackageFile entryPointLayout;
        static readonly PackageBuilderTypeDescriptionProvider descriptionProvider = new PackageBuilderTypeDescriptionProvider();

        public GalleryPackageBuilderDialog()
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
            var manifest = Manifest.Create(packageBuilder);
            manifest.Metadata.DependencyGroups = null;
            if (metadataExists)
            {
                using var stream = File.OpenRead(metadataPath);
                var existingManifest = Manifest.ReadFrom(stream, true);
                manifest.Metadata.DependencyGroups = existingManifest.Metadata.DependencyGroups;
                if (existingManifest.Files is not null)
                {
                    manifest.Files.AddRange(existingManifest.Files);
                }
            }

            using (var stream = File.Open(metadataPath, FileMode.Create))
            {
                manifest.Save(stream);
                metadataSaveVersion = metadataVersion;
                return true;
            }
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
            if (!EnsureMetadata())
            {
                DialogResult = DialogResult.None;
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        private bool EnsureMetadata()
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
                    return false;
                }
            }

            return true;
        }

        private bool ExportPackage()
        {
            if (!EnsureMetadata())
                return false;

            var packageFileName =
                packageBuilder.Id + "." +
                packageBuilder.Version + NuGetConstants.PackageExtension;
            saveFileDialog.FileName = packageFileName;
            if (entryPoint != null)
            {
                RenamePackageFile(entryPoint, packageBuilder.Id);
                if (entryPointLayout != null) RenamePackageFile(entryPointLayout, packageBuilder.Id);
            }

            EnsureDirectory(saveFileDialog.InitialDirectory);
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return false;

            using var dialog = new PackageOperationDialog();
            var logger = new EventLogger();
            dialog.Text = Resources.ExportOperationLabel;
            dialog.RegisterEventLogger(logger);
            logger.Log(LogLevel.Information, $"Creating package '{packageBuilder.Id} {packageBuilder.Version}'.");
            var dialogClosed = Observable.FromEventPattern<FormClosedEventHandler, FormClosedEventArgs>(
                handler => dialog.FormClosed += handler,
                handler => dialog.FormClosed -= handler);
            var operation = Observable.Using(
                () => Stream.Synchronized(File.Open(saveFileDialog.FileName, FileMode.Create)),
                stream => Observable.Start(() => packageBuilder.Save(stream)).TakeUntil(dialogClosed));

            using var subscription = operation.ObserveOn(this).Subscribe(
                xs => dialog.Complete(),
                ex => logger.Log(LogLevel.Error, ex.Message));
            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            SystemSounds.Asterisk.Play();
            var message = string.Format(Resources.PackageExported, packageBuilder.Id, packageBuilder.Version);
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.None);
            return true;
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

        private void ExportButton_Click(object sender, EventArgs e)
        {
            ExportPackage();
        }
    }
}
