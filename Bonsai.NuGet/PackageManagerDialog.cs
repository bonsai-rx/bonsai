using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageManagerDialog : Form
    {
        const string DefaultRepository = "Bonsai Packages";
        PackageManagerProxy packageManagerProxy;
        PackageViewController packageViewController;

        public PackageManagerDialog(string path)
        {
            InitializeComponent();
            packageManagerProxy = new PackageManagerProxy();
            packageManagerProxy.PackageInstalling += packageManagerProxy_PackageInstalling;
            packageViewController = new PackageViewController(
                path,
                this,
                packageView,
                packageDetails,
                packagePageSelector,
                packageManagerProxy,
                packageIcons,
                searchComboBox,
                prereleaseCheckBox,
                () => updatesButton.Checked,
                value => multiOperationPanel.Visible = value,
                Enumerable.Empty<string>());
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
            get { return packageManagerProxy; }
        }

        private void InitializePackageSourceItems()
        {
            packageSourceComboBox.Items.Clear();
            foreach (var pair in packageViewController.PackageManagers)
            {
                packageSourceComboBox.Items.Add(pair);
            }
        }

        private void UpdateSelectedRepository()
        {
            packageViewController.SetPackageViewStatus(Resources.NoItemsFoundLabel);
            packageViewController.ClearActiveRequests();
            if (installedButton.Checked)
            {
                packageView.OperationText = Resources.UninstallOperationName;
                packageViewController.SelectedRepository = packageManagerProxy.LocalRepository;
            }
            else
            {
                var selectedItem = packageSourceComboBox.SelectedItem;
                if (selectedItem != null)
                {
                    var selectedManager = ((KeyValuePair<string, PackageManager>)selectedItem).Value;
                    packageViewController.SelectedRepository = selectedManager.SourceRepository;
                }
                else if (packageViewController.PackageManagers.TryGetValue(DefaultRepository, out PackageManager defaultPackageManager))
                {
                    packageViewController.SelectedRepository = defaultPackageManager.SourceRepository;
                }
                else packageViewController.SelectedRepository = packageViewController.PackageManagers[Resources.AllNodeName].SourceRepository;

                if (updatesButton.Checked)
                {
                    packageView.OperationText = Resources.UpdateOperationName;
                }
                else packageView.OperationText = Resources.InstallOperationName;
            }

            searchComboBox.Text = string.Empty;
            packageViewController.UpdatePackageFeed();
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
            if (packageView.OperationText == Resources.UpdateOperationName)
            {
                var packageFeed = packageViewController.GetPackageFeed();
                var packages = packageFeed()
                    .AsEnumerable()
                    .Where(PackageExtensions.IsListed)
                    .AsCollapsed();
                packageViewController.RunPackageOperation(packages, true);
            }
        }

        private void packageView_OperationClick(object sender, TreeViewEventArgs e)
        {
            bool handleDependencies = true;
            var package = (IPackage)e.Node.Tag;
            if (package != null)
            {
                if (packageViewController.SelectedRepository == packageManagerProxy.LocalRepository)
                {
                    var dependencies = (from dependency in package.GetCompatiblePackageDependencies(null)
                                        let dependencyPackage = packageViewController.SelectedRepository.ResolveDependency(dependency, true, true)
                                        where dependencyPackage != null
                                        select dependencyPackage)
                                        .ToArray();
                    if (dependencies.Length > 0)
                    {
                        var dependencyNotice = new StringBuilder();
                        dependencyNotice.AppendLine(string.Format(Resources.PackageDependencyNotice, package));
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

        void packageManagerProxy_PackageInstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var entryPoint = package.Id + Constants.BonsaiExtension;
            if (package.GetContentFiles().Any(file => file.EffectivePath == entryPoint))
            {
                Invoke((Action)(() =>
                {
                    var message = string.Format(Resources.InstallExecutablePackageWarning, package.Id);
                    var result = MessageBox.Show(this, message, Resources.InstallExecutablePackageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.Yes)
                    {
                        saveFolderDialog.FileName = package.Id;
                        if (saveFolderDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            var targetPath = saveFolderDialog.FileName;
                            var targetFileSystem = new PhysicalFileSystem(targetPath);
                            InstallPath = PackageHelper.InstallExecutablePackage(package, targetFileSystem);
                            DialogResult = DialogResult.OK;
                        }
                    }
                }));
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
    }

    public enum PackageManagerTab
    {
        Browse,
        Installed,
        Updates
    }
}
