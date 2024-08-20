using System;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using Bonsai.NuGet.Design.Properties;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging;
using System.IO;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using System.ComponentModel;

namespace Bonsai.NuGet.Design
{
    internal partial class PackageDetails : UserControl
    {
        PackageViewItem selectedItem;
        PackageOperationType operation;
        const int TextHeightMargin = 7;
        static readonly object OperationClickEvent = new();

        public PackageDetails()
        {
            InitializeComponent();
            ProjectFramework = NuGetFramework.AnyFramework;
            SetPackage(null);
        }

        public PackageOperationType Operation
        {
            get => operation;
            set
            {
                operation = value;
                operationButton.Text = value.ToString();
            }
        }

        [Category("Action")]
        public event PackageViewEventHandler OperationClick
        {
            add { Events.AddHandler(OperationClickEvent, value); }
            remove { Events.RemoveHandler(OperationClickEvent, value); }
        }

        public NuGetFramework ProjectFramework { get; set; }

        public PackagePathResolver PathResolver { get; set; }

        private void OnOperationClick(PackageViewEventArgs e)
        {
            (Events[OperationClickEvent] as PackageViewEventHandler)?.Invoke(this, e);
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

            var package = item.Package;
            packageIdLabel.ImageList = item.ImageList;
            packageIdLabel.ImageIndex = item.ImageIndex;
            packageIdLabel.Text = package.Identity.Id;

            createdByLabel.Text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
            SetLinkLabelUri(detailsLinkLabel, package.PackageDetailsUrl, true);
            versionLabel.Text = package.Identity.Version.ToString();
            lastPublishedLabel.Text = package.Published.HasValue ? package.Published.Value.Date.ToShortDateString() : Resources.UnpublishedLabel;
            downloadsLabel.Text = package.DownloadCount.ToString();
            SetLinkLabelLicense(licenseLinkLabel, package, true);
            SetLinkLabelUri(projectLinkLabel, package.ProjectUrl, true);
            SetLinkLabelUri(reportAbuseLinkLabel, package.ReportAbuseUrl, false);
            descriptionLabel.Text = package.Description;
            tagsLabel.Text = package.Tags;

            var deprecationMetadata = package.GetDeprecationMetadataAsync().Result;
            if (deprecationMetadata != null)
            {
                deprecationMetadataPanel.Visible = true;
                deprecationMetadataLabel.Text = deprecationMetadata.Message;
            }
            else
            {
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
            ResumeLayout();
        }

        void SetLinkLabelLicense(LinkLabel linkLabel, IPackageSearchMetadata package, bool hideEmptyLink)
        {
            var license = package.LicenseMetadata;
            if (license != null && PathResolver != null)
            {
                switch (license.Type)
                {
                    case LicenseType.File:
                        var licenseUri = new Uri(Path.Combine(PathResolver.GetInstallPath(package.Identity), license.License));
                        SetLinkLabelUri(linkLabel, licenseUri, hideEmptyLink);
                        break;
                    case LicenseType.Expression: SetLinkLabelUri(linkLabel, license.LicenseUrl, hideEmptyLink); break;
                    default: break;
                }
            }
            else SetLinkLabelUri(linkLabel, package.LicenseUrl, hideEmptyLink);
        }

        static void SetLinkLabelUri(LinkLabel linkLabel, Uri uri, bool hideEmptyLink)
        {
            linkLabel.Links[0].Description = uri != null && uri.IsAbsoluteUri ? uri.AbsoluteUri : null;
            linkLabel.Links[0].LinkData = uri;
            linkLabel.Visible = !hideEmptyLink || linkLabel.Links[0].LinkData != null;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkUri = (Uri)e.Link.LinkData;
            if (linkUri != null)
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
            OnOperationClick(new PackageViewEventArgs(selectedItem.Package, Operation));
        }
    }
}
