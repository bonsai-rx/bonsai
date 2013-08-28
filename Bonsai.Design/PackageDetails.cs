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

namespace Bonsai.Design
{
    public partial class PackageDetails : UserControl
    {
        static readonly Uri NugetPackageRepository = new Uri("https://packages.nuget.org/api/v2");
        const string NoDependenciesText = "No Dependencies";
        const string DependencyWarningText = "Each item above may have sub-dependencies subject to additional license agreements.";

        public PackageDetails()
        {
            InitializeComponent();
            SetPackage(null);
        }

        public void SetPackage(IPackage package)
        {
            detailsLayoutPanel.Visible = package != null;
            if (package == null) return;

            createdByLabel.Text = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
            idLinkLabel.Text = package.Id;
            var packageUri = new Uri(NugetPackageRepository, package.Id + "/" + package.Version.ToString());
            SetLinkLabelUri(idLinkLabel, packageUri, false);
            versionLabel.Text = package.Version.ToString();
            lastPublishedLabel.Text = package.Published.HasValue ? package.Published.Value.Date.ToShortDateString() : "(unpublished)";
            downloadsLabel.Text = package.DownloadCount.ToString();
            SetLinkLabelUri(licenseLinkLabel, package.LicenseUrl, true);
            SetLinkLabelUri(projectLinkLabel, package.ProjectUrl, true);
            SetLinkLabelUri(reportAbuseLinkLabel, package.ReportAbuseUrl, false);
            descriptionLabel.Text = package.Description;
            tagsLabel.Text = package.Tags;
            dependenciesTextBox.Lines = (from dependencySet in package.DependencySets
                                         from dependency in dependencySet.Dependencies
                                         select PackageDependencyString(dependency)).ToArray();
            if (dependenciesTextBox.Lines.Length > 0)
            {
                dependenciesTextBox.Visible = true;
                dependencyWarningLabel.Text = DependencyWarningText;
            }
            else
            {
                dependenciesTextBox.Visible = false;
                dependencyWarningLabel.Text = NoDependenciesText;
            }
        }

        static void SetLinkLabelUri(LinkLabel linkLabel, Uri uri, bool hideEmptyLink)
        {
            linkLabel.Links[0].Description = uri != null && uri.IsAbsoluteUri ? uri.AbsoluteUri : null;
            linkLabel.Links[0].LinkData = uri;
            linkLabel.Visible = !hideEmptyLink || linkLabel.Links[0].LinkData != null;
        }

        static string PackageDependencyString(PackageDependency dependency)
        {
            var versionSpecText = VersionSpecString(dependency.VersionSpec);
            return dependency.Id + versionSpecText;
        }

        static string VersionSpecString(IVersionSpec versionSpec)
        {
            if (versionSpec == null) return string.Empty;
            var versionText = new StringBuilder();
            if (versionSpec.MinVersion != null)
            {
                if (versionSpec.MaxVersion == versionSpec.MinVersion)
                {
                    versionText.Append("= ");
                }
                else versionText.Append(versionSpec.IsMinInclusive ? "≥ " : "> ");
                versionText.Append(versionSpec.MinVersion);
            }

            if (versionSpec.MaxVersion != null && versionSpec.MaxVersion != versionSpec.MinVersion)
            {
                if (versionSpec.MinVersion != null)
                {
                    versionText.Append(" && ");
                }

                versionText.Append(versionSpec.IsMaxInclusive ? "≤ " : "< ");
                versionText.Append(versionSpec.MaxVersion);
            }

            if (versionText.Length > 0)
            {
                return string.Format(" ({0})", versionText);
            }
            else return string.Empty;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkUri = (Uri)e.Link.LinkData;
            if (linkUri != null)
            {
                Process.Start(linkUri.AbsoluteUri);
            }
        }
    }
}
