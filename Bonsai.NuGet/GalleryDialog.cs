using Bonsai.NuGet.Properties;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class GalleryDialog : Form
    {
        PackageViewController packageViewController;

        string targetPath;
        PackageIdentity targetPackage;

        public GalleryDialog(string path)
        {
            InitializeComponent();
            packageViewController = new PackageViewController(
                path,
                this,
                packageView,
                packageDetails,
                packagePageSelector,
                packageIcons,
                searchComboBox,
                prereleaseCheckBox,
                () => false,
                value => { },
                new[] { Constants.GalleryPackageType });
            packageViewController.SearchPrefix = $"tags:{Constants.GalleryDirectory} ";
            packageViewController.PackageManager.PackageManagerPlugins.Add(new GalleryPackagePlugin(this));
            InitializePackageSourceItems();
        }

        public string InstallPath { get; set; }

        public IPackageManager PackageManager
        {
            get { return packageViewController.PackageManager; }
        }

        protected override void OnLoad(EventArgs e)
        {
            packageViewController.OnLoad(e);
            UpdateSelectedRepository();
            searchComboBox.Select();
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            const int MaxImageSize = 256;
            packageView.ItemHeight = (int)(64 * factor.Height);
            packageIcons.ImageSize = new Size(
                Math.Min(MaxImageSize, (int)(32 * factor.Height)),
                Math.Min(MaxImageSize, (int)(32 * factor.Height)));
            base.ScaleControl(factor, specified);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            packageViewController.OnHandleDestroyed(e);
            base.OnHandleDestroyed(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            packageViewController.OnResizeBegin(e);
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            packageViewController.OnResizeEnd(e);
            base.OnResizeEnd(e);
        }

        private void packageView_OperationClick(object sender, TreeViewEventArgs e)
        {
            var package = (IPackageSearchMetadata)e.Node.Tag;
            if (package != null)
            {
                saveFolderDialog.FileName = package.Identity.Id;
                if (saveFolderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    targetPackage = package.Identity;
                    targetPath = saveFolderDialog.FileName;
                    packageViewController.RunPackageOperation(new[] { package }, true);
                    if (DialogResult == DialogResult.OK)
                    {
                        Close();
                    }
                }
            }
        }

        class GalleryPackagePlugin : PackageManagerPlugin
        {
            public GalleryPackagePlugin(GalleryDialog owner)
            {
                Owner = owner;
            }

            private GalleryDialog Owner { get; set; }

            public override Task<bool> OnPackageInstallingAsync(PackageIdentity package, PackageReaderBase packageReader, string installPath)
            {
                if (PackageIdentityComparer.Default.Equals(package, Owner.targetPackage))
                {
                    var framework = Owner.packageViewController.PackageManager.ProjectFramework;
                    Owner.InstallPath = PackageHelper.InstallExecutablePackage(package, framework, packageReader, Owner.targetPath);
                    Owner.DialogResult = DialogResult.OK;
                }

                return base.OnPackageInstallingAsync(package, packageReader, installPath);
            }
        }

        private void InitializePackageSourceItems()
        {
            packageSourceComboBox.Items.Clear();
            foreach (var repository in PackageManager.SourceRepositoryProvider.GetRepositories())
            {
                packageSourceComboBox.Items.Add(repository);
            }
        }

        private void UpdateSelectedRepository()
        {
            packageViewController.SetPackageViewStatus(Resources.NoItemsFoundLabel);
            packageViewController.ClearActiveRequests();

            var selectedItem = packageSourceComboBox.SelectedItem;
            if (selectedItem != null)
            {
                packageViewController.SelectedRepository = (SourceRepository)selectedItem;
            }
            else packageViewController.SelectedRepository = null;

            packageView.OperationText = Resources.OpenOperationName;
            searchComboBox.Text = string.Empty;
            packageViewController.UpdatePackagePage();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            UpdateSelectedRepository();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            Hide();
            if (packageViewController.ShowPackageSourceConfigurationDialog() == DialogResult.OK)
            {
                UpdateSelectedRepository();
            }
            Show();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
