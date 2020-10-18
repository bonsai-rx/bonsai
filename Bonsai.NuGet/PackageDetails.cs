using System;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using Bonsai.NuGet.Properties;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet
{
    public partial class PackageDetails : UserControl
    {
        const int TextHeightMargin = 7;
        static readonly Uri NugetPackageRepository = new Uri("https://packages.nuget.org/packages/");

        public PackageDetails()
        {
            InitializeComponent();
            SetPackage(null);
        }

        public void SetPackage(IPackageSearchMetadata package)
        {
            SuspendLayout();
            detailsLayoutPanel.Visible = package != null;
            if (package == null) return;

            createdByLabel.Text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
            idLinkLabel.Text = package.Identity.Id;
            var packageUri = package.PackageDetailsUrl ?? new Uri(NugetPackageRepository, package.Identity.Id + "/" + package.Identity.Version.ToString());
            SetLinkLabelUri(idLinkLabel, packageUri, false);
            versionLabel.Text = string.Format(
                "{0}{1}",
                package.Identity.Version.ToString(),
                package.Identity.Version.IsPrerelease ? Resources.PrereleaseLabel : string.Empty);
            lastPublishedLabel.Text = package.Published.HasValue ? package.Published.Value.Date.ToShortDateString() : Resources.UnpublishedLabel;
            downloadsLabel.Text = package.DownloadCount.ToString();
            SetLinkLabelUri(licenseLinkLabel, package.LicenseUrl, true);
            SetLinkLabelUri(projectLinkLabel, package.ProjectUrl, true);
            SetLinkLabelUri(reportAbuseLinkLabel, package.ReportAbuseUrl, false);
            descriptionLabel.Text = package.Description;
            tagsLabel.Text = package.Tags;
            dependenciesTextBox.Lines = (from dependencySet in package.DependencySets
                                         from dependency in dependencySet.Packages
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

        private void dependenciesTextBox_TextChanged(object sender, EventArgs e)
        {
            var textSize = TextRenderer.MeasureText(dependenciesTextBox.Text, dependenciesTextBox.Font);
            textSize.Height += TextHeightMargin;
            dependenciesTextBox.Size = textSize;
        }
    }
}
