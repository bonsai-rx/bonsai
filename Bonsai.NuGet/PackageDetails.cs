using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NuGet;
using System.Globalization;
using System.Diagnostics;
using Bonsai.NuGet.Properties;

namespace Bonsai.NuGet
{
    public partial class PackageDetails : UserControl
    {
        const int TextHeightMargin = 7;
        static readonly Uri NugetPackageRepository = new Uri("https://packages.nuget.org/api/v2");

        public PackageDetails()
        {
            InitializeComponent();
            SetPackage(null);
        }

        public void SetPackage(IPackage package)
        {
            SuspendLayout();
            detailsLayoutPanel.Visible = package != null;
            if (package == null) return;

            createdByLabel.Text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
            idLinkLabel.Text = package.Id;
            var packageUri = new Uri(NugetPackageRepository, package.Id + "/" + package.Version.ToString());
            SetLinkLabelUri(idLinkLabel, packageUri, false);
            versionLabel.Text = string.Format(
                "{0}{1}",
                package.Version.ToString(),
                package.IsReleaseVersion() ? string.Empty : Resources.PrereleaseLabel);
            lastPublishedLabel.Text = package.Published.HasValue ? package.Published.Value.Date.ToShortDateString() : Resources.UnpublishedLabel;
            downloadsLabel.Text = package.DownloadCount.ToString();
            SetLinkLabelUri(licenseLinkLabel, package.LicenseUrl, true);
            SetLinkLabelUri(projectLinkLabel, package.ProjectUrl, true);
            SetLinkLabelUri(reportAbuseLinkLabel, package.ReportAbuseUrl, false);
            descriptionLabel.Text = package.Description;
            tagsLabel.Text = package.Tags;
            dependenciesTextBox.Lines = (from dependencySet in package.DependencySets
                                         from dependency in dependencySet.Dependencies
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
