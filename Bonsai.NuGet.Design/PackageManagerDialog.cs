using Bonsai.NuGet.Design.Properties;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public partial class PackageManagerDialog : Form
    {
        const string AggregateRepository = "All";
        const string DefaultRepository = "Bonsai Packages";
        readonly PackageViewController packageViewController;

        public PackageManagerDialog(NuGetFramework projectFramework, string path)
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
                () => updatesButton.Checked,
                value => multiOperationPanel.Visible = value);
            packageViewController.PackageTypes = new[] { Constants.LibraryPackageType };
            packageViewController.PackageManager.PackageManagerPlugins.Add(new ExecutablePackagePlugin(this));
            InitializePackageSourceItems();
            multiOperationPanel.Visible = false;
            multiOperationLabel.Text = Resources.MultipleUpdatesLabel;
            multiOperationButton.Text = Resources.MultipleUpdatesOperationName;
            DefaultTab = PackageManagerTab.Browse;
        }

        public PackageManagerTab DefaultTab { get; set; }

        public string InstallPath { get; set; }

        public IPackageManager PackageManager
        {
            get { return packageViewController.PackageManager; }
        }

        private void InitializePackageSourceItems()
        {
            packageSourceComboBox.Items.Clear();
            packageSourceComboBox.Items.Add(AggregateRepository);
            packageSourceComboBox.SelectedIndex = 0;

            foreach (var repository in packageViewController.PackageManager.SourceRepositoryProvider.GetRepositories())
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
            if (installedButton.Checked)
            {
                packageViewController.Operation = PackageOperationType.Uninstall;
                packageViewController.SelectedRepository = PackageManager.LocalRepository;
            }
            else
            {
                var selectedItem = packageSourceComboBox.SelectedItem;
                if (!AggregateRepository.Equals(selectedItem))
                {
                    packageViewController.SelectedRepository = (SourceRepository)selectedItem;
                }
                else packageViewController.SelectedRepository = null;

                if (updatesButton.Checked)
                {
                    packageViewController.Operation = PackageOperationType.Update;
                }
                else packageViewController.Operation = PackageOperationType.Install;
            }

            searchComboBox.Text = string.Empty;
            packageViewController.UpdatePackageQuery();
        }

        protected override void OnLoad(EventArgs e)
        {
            packageViewController.OnLoad(e);
            switch (DefaultTab)
            {
                case PackageManagerTab.Installed: installedButton.PerformClick(); break;
                case PackageManagerTab.Updates: updatesButton.PerformClick(); break;
                default: browseButton.PerformClick(); break;
            }
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

        private void multiOperationButton_Click(object sender, EventArgs e)
        {
            if (packageViewController.Operation == PackageOperationType.Update)
            {
                var packages = packageView.Nodes.Cast<TreeNode>()
                    .Select(node => node.Tag as IPackageSearchMetadata)
                    .Where(package => package != null)
                    .ToList();
                packageViewController.RunPackageOperation(packages, true);
            }
        }

        private void packageView_OperationClick(object sender, PackageViewEventArgs e)
        {
            bool handleDependencies = true;
            var package = e.Package;
            if (package != null)
            {
                if (packageViewController.SelectedRepository == PackageManager.LocalRepository)
                {
                    var nearestDependencyGroup = package.DependencySets.GetNearest(packageViewController.ProjectFramework);
                    var dependencies = ((nearestDependencyGroup?.Packages) ?? Enumerable.Empty<PackageDependency>()).ToList();
                    if (dependencies.Count > 0)
                    {
                        var dependencyNotice = new StringBuilder();
                        dependencyNotice.AppendLine(string.Format(Resources.PackageDependencyNotice, package.Identity));
                        foreach (var dependency in dependencies)
                        {
                            dependencyNotice.AppendLine(dependency.ToString());
                        }

                        var result = MessageBox.Show(this, dependencyNotice.ToString(), Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Cancel) return;
                        if (result == DialogResult.No) handleDependencies = false;
                    }
                }

                packageViewController.RunPackageOperation(new[] { package }, handleDependencies);
                if (DialogResult == DialogResult.OK)
                {
                    Close();
                }
            }
        }

        class ExecutablePackagePlugin : PackageManagerPlugin
        {
            public ExecutablePackagePlugin(PackageManagerDialog owner)
            {
                Owner = owner;
            }

            public PackageManagerDialog Owner { get; set; }

            public override Task<bool> OnPackageInstallingAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                if (packageReader.IsExecutablePackage(package, projectFramework))
                {
                    Owner.Invoke((Action)(() =>
                    {
                        var message = string.Format(Resources.InstallExecutablePackageWarning, package.Id);
                        var result = MessageBox.Show(Owner, message, Resources.InstallExecutablePackageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Yes)
                        {
                            Owner.saveFolderDialog.FileName = package.Id;
                            if (Owner.saveFolderDialog.ShowDialog(Owner) == DialogResult.OK)
                            {
                                var targetPath = Owner.saveFolderDialog.FileName;
                                Owner.InstallPath = packageReader.InstallExecutablePackage(package, projectFramework, targetPath);
                                Owner.DialogResult = DialogResult.OK;
                            }
                        }
                    }));
                }

                return base.OnPackageInstallingAsync(package, projectFramework, packageReader, installPath);
            }
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

        private void dependencyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var packageType = dependencyCheckBox.Checked
                ? PackageType.Dependency.Name
                : Constants.LibraryPackageType;
            packageViewController.PackageTypes = new[] { packageType };
            packageViewController.UpdatePackageQuery();
        }
    }

    public enum PackageManagerTab
    {
        Browse,
        Installed,
        Updates
    }
}
