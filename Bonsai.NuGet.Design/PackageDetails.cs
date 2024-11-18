using System;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using Bonsai.NuGet.Design.Properties;
using NuGet.Protocol.Core.Types;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using System.ComponentModel;
using NuGet.Versioning;
using System.Drawing;

namespace Bonsai.NuGet.Design
{
    internal partial class PackageDetails : UserControl
    {
        PackageViewItem selectedItem;
        const int TextHeightMargin = 7;
        static readonly object OperationClickEvent = new();
        static readonly object PackageLinkClickedEvent = new();

        public PackageDetails()
        {
            InitializeComponent();
            ProjectFramework = NuGetFramework.AnyFramework;
            versionComboBox.DisplayMember = nameof(VersionInfo.Version);
            toolTip.SetToolTip(prefixReservedIcon, Resources.PackagePrefixReservedToolTip);
            SetPackage(null);
        }

        [Category("Action")]
        public event PackageViewEventHandler OperationClick
        {
            add { Events.AddHandler(OperationClickEvent, value); }
            remove { Events.RemoveHandler(OperationClickEvent, value); }
        }

        [Category("Action")]
        public event PackageSearchEventHandler PackageLinkClicked
        {
            add { Events.AddHandler(PackageLinkClickedEvent, value); }
            remove { Events.RemoveHandler(PackageLinkClickedEvent, value); }
        }

        public NuGetFramework ProjectFramework { get; set; }

        private PackageOperationType Operation => installedVersionLayoutPanel.Visible
            ? PackageOperationType.Update
            : PackageOperationType.Install;

        private void OnOperationClick(PackageViewEventArgs e)
        {
            (Events[OperationClickEvent] as PackageViewEventHandler)?.Invoke(this, e);
        }

        private void OnPackageLinkClicked(PackageSearchEventArgs e)
        {
            (Events[PackageLinkClickedEvent] as PackageSearchEventHandler)?.Invoke(this, e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if (factor.Height > 1)
            {
                var image = Resources.PrefixReservedImage;
                var targetSize = new Size(
                    (int)(image.Width * factor.Height),
                    (int)(image.Height * factor.Height));
                prefixReservedIcon.Image = image.Resize(targetSize);
            }
            base.ScaleControl(factor, specified);
        }

        public void SetPackage(PackageViewItem item)
        {
            SuspendLayout();
            selectedItem = item;
            detailsLayoutPanel.Visible = item != null;
            if (item == null)
            {
                packageIdLabel.ImageList = null;
                packageIdLabel.ImageIndex = 0;
                packageIdLabel.Text = string.Empty;
                ResumeLayout();
                return;
            }

            var package = item.SelectedPackage;
            packageIdLabel.ImageList = item.ImageList;
            packageIdLabel.ImageIndex = item.ImageIndex;
            packageIdLabel.Text = package.Identity.Id;
            prefixReservedIcon.Visible = package.PrefixReserved;

            installedVersionLayoutPanel.Visible = selectedItem.LocalPackage != null;
            if (installedVersionLayoutPanel.Visible)
            {
                installedVersionTextBox.Text = selectedItem.LocalPackage.Identity.Version.ToString();
            }
            operationButton.Text = Operation.ToString();

            versionComboBox.Items.Clear();
            foreach (var version in item.PackageVersions
                                        .OrderByDescending(v => v.Version, VersionComparer.VersionRelease))
            {
                versionComboBox.Items.Add(version);
                if (version.Version == package.Identity.Version)
                {
                    version.PackageSearchMetadata = package;
                    versionComboBox.SelectedItem = version;
                }
            }

            var selectedVersion = (VersionInfo)versionComboBox.SelectedItem;
            SetPackageVersion(selectedVersion);
            ResumeLayout();
        }

        void SetPackageVersion(VersionInfo versionInfo)
        {
            var package = versionInfo.PackageSearchMetadata;
            operationButton.Enabled = !(selectedItem.LocalPackage?.Identity.Version == versionInfo.Version);
            createdByLabel.Text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
            SetLinkLabelUri(detailsLinkLabel, package.PackageDetailsUrl, true);
            lastPublishedLabel.Text = package.Published.HasValue ? package.Published.Value.Date.ToShortDateString() : Resources.UnpublishedLabel;
            downloadsLabel.Text = versionInfo.DownloadCount.ToString();
            LicenseHelper.SetLicenseLinkLabel(licenseLinkLabel, package, selectedItem.SourceRepository);
            SetLinkLabelUri(projectLinkLabel, package.ProjectUrl, true);
            SetLinkLabelUri(reportAbuseLinkLabel, package.ReportAbuseUrl, false);
            descriptionLabel.Text = package.Description;
            tagsLabel.Text = package.Tags;

            var deprecationMetadata = package.GetDeprecationMetadataAsync().Result;
            if (deprecationMetadata != null)
            {
                deprecationMetadataPanel.Visible = true;
                deprecationMetadataLabel.Text = string.IsNullOrEmpty(deprecationMetadata.Message)
                    ? Resources.PackageDeprecationDefaultMessage
                    : deprecationMetadata.Message;

                alternatePackagePanel.Visible = deprecationMetadata.AlternatePackage != null;
                if (alternatePackagePanel.Visible)
                {
                    var alternatePackageId = deprecationMetadata.AlternatePackage.PackageId;
                    alternatePackageLinkLabel.Text = alternatePackageId;
                    alternatePackageLinkLabel.Links[0].LinkData = $"packageid:{alternatePackageId}";
                }
            }
            else
            {
                alternatePackagePanel.Visible = false;
                deprecationMetadataPanel.Visible = false;
                deprecationMetadataLabel.Text = string.Empty;
            }

            var nearestDependencyGroup = package.DependencySets.GetNearest(ProjectFramework);
            dependenciesTextBox.Lines = (from dependency in ((nearestDependencyGroup?.Packages) ?? Enumerable.Empty<PackageDependency>())
                                         select dependency.ToString()).ToArray();
            if (dependenciesTextBox.Lines.Length > 0)
            {
                dependenciesTextBox.Visible = true;
                dependencyWarningLabel.Text = Resources.DependencyLicenseWarningLabel;
            }
            else
            {
                dependenciesTextBox.Visible = false;
                dependencyWarningLabel.Text = Resources.NoDependenciesLabel;
            }
        }

        static void SetLinkLabelUri(LinkLabel linkLabel, Uri uri, bool hideEmptyLink)
        {
            linkLabel.Links[0].Description = uri != null && uri.IsAbsoluteUri ? uri.AbsoluteUri : null;
            linkLabel.Links[0].LinkData = uri;
            linkLabel.Visible = !hideEmptyLink || linkLabel.Links[0].LinkData != null;
        }

        private async void licenseLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            await LicenseHelper.ShowLicenseAsync(e.Link, this);
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.LinkData is Uri linkUri)
            {
                Process.Start(linkUri.AbsoluteUri);
            }
        }

        private void linkLabel_MouseEnter(object sender, EventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            linkLabel.LinkColor = ControlPaint.Light(linkLabel.ForeColor);
        }

        private void linkLabel_MouseLeave(object sender, EventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            linkLabel.LinkColor = linkLabel.ForeColor;
        }

        private void dependenciesTextBox_TextChanged(object sender, EventArgs e)
        {
            var textSize = TextRenderer.MeasureText(dependenciesTextBox.Text, dependenciesTextBox.Font);
            textSize.Height += TextHeightMargin;
            dependenciesTextBox.Size = textSize;
        }

        private void operationButton_Click(object sender, EventArgs e)
        {
            var selectedVersion = (VersionInfo)versionComboBox.SelectedItem;
            if (selectedVersion != null)
            {
                OnOperationClick(new PackageViewEventArgs(selectedVersion.PackageSearchMetadata, Operation));
            }
        }

        private void uninstallButton_Click(object sender, EventArgs e)
        {
            var metadataBuilder = PackageSearchMetadataBuilder.FromIdentity(selectedItem.LocalPackage.Identity);
            OnOperationClick(new PackageViewEventArgs(metadataBuilder.Build(), PackageOperationType.Uninstall));
        }

        private void alternatePackageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var searchTerm = (string)e.Link.LinkData;
            OnPackageLinkClicked(new PackageSearchEventArgs(searchTerm));
        }

        private async void versionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currentPackage = selectedItem;
            var selectedVersion = (VersionInfo)versionComboBox.SelectedItem;
            selectedVersion.PackageSearchMetadata ??= await selectedItem.GetVersionMetadataAsync(selectedVersion);

            // cancel the update if selected package changes before version metadata is extracted
            if (currentPackage != selectedItem || selectedVersion.PackageSearchMetadata is null)
                return;

            SuspendLayout();
            SetPackageVersion(selectedVersion);
            ResumeLayout();
        }

        private void versionComboBox_TextChanged(object sender, EventArgs e)
        {
            if (versionComboBox.SelectedIndex < 0)
            {
                operationButton.Enabled = false;
            }
        }
    }
}
