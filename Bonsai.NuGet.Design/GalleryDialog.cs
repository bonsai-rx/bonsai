using Bonsai.NuGet.Design.Properties;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public partial class GalleryDialog : Form
    {
        const string AggregateRepository = "All";
        const string DefaultRepository = "Community Packages";
        readonly PackageViewController packageViewController;

        string targetPath;
        PackageIdentity targetPackage;

        public GalleryDialog(NuGetFramework projectFramework, string path)
        {
            InitializeComponent();
            packageViewController = new PackageViewController(
                projectFramework,
                path,
                this,
                packageView,
                packageDetails,
                packagePageSelector,
                packageIcons,
                searchComboBox,
                prereleaseCheckBox,
                () => false,
                value => { });
            packageViewController.SearchPrefix = $"tags:{Constants.GalleryDirectory} ";
            packageViewController.PackageTypes = new[] { Constants.GalleryPackageType };
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

        private void packageView_OperationClick(object sender, PackageViewEventArgs e)
        {
            var package = e.Package;
            if (package != null)
            {
                if (!package.Tags.Contains(Constants.GalleryDirectory))
                {
                    MessageBox.Show(this,
                        string.Format(Resources.InvalidGalleryPackage, package.Identity),
                        string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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

            public override Task<bool> OnPackageInstallingAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                if (PackageIdentityComparer.Default.Equals(package, Owner.targetPackage))
                {
                    Owner.InstallPath = packageReader.InstallExecutablePackage(package, projectFramework, Owner.targetPath);
                    Owner.DialogResult = DialogResult.OK;
                }

                return base.OnPackageInstallingAsync(package, projectFramework, packageReader, installPath);
            }
        }

        private void InitializePackageSourceItems()
        {
            packageSourceComboBox.Items.Clear();
            packageSourceComboBox.Items.Add(AggregateRepository);
            packageSourceComboBox.SelectedIndex = 0;

            foreach (var repository in PackageManager.SourceRepositoryProvider.GetRepositories())
            {
                packageSourceComboBox.Items.Add(repository);
                if (repository.PackageSource.Name == DefaultRepository)
                {
                    packageSourceComboBox.SelectedIndex = packageSourceComboBox.Items.Count - 1;
                }
            }
        }

        private void UpdateSelectedRepository()
        {
            packageViewController.SetPackageViewStatus(Resources.NoItemsFoundLabel);
            packageViewController.ClearActiveRequests();

            var selectedItem = packageSourceComboBox.SelectedItem;
            if (!AggregateRepository.Equals(selectedItem))
            {
                packageViewController.SelectedRepository = (SourceRepository)selectedItem;
            }
            else packageViewController.SelectedRepository = null;

            searchComboBox.Text = string.Empty;
            packageViewController.UpdatePackageQuery();
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
                InitializePackageSourceItems();
            }
            Show();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
