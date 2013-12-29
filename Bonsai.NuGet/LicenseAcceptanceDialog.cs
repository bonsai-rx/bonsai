using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class LicenseAcceptanceDialog : Form
    {
        public LicenseAcceptanceDialog(IEnumerable<IPackage> licensePackages)
        {
            if (licensePackages == null)
            {
                throw new ArgumentNullException("licensePackages");
            }

            InitializeComponent();
            SuspendLayout();
            var bold = new Font(Font, FontStyle.Bold);
            foreach (var package in licensePackages)
            {
                var titleAuthorshipPanel = new FlowLayoutPanel();
                var titleLabel = new Label();
                var authorshipLabel = new Label();
                var viewLicenseLabel = new LinkLabel();
                viewLicenseLabel.LinkClicked += viewLicenseLabel_LinkClicked;

                titleLabel.Font = bold;
                titleLabel.AutoSize = true;
                titleLabel.Text = package.Id;
                authorshipLabel.AutoSize = true;
                var authorsText = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
                authorshipLabel.Text = string.Format(Resources.LicenseAuthorshipLabel, authorsText);
                viewLicenseLabel.Text = Resources.LicenseViewLicenseLabel;
                SetLinkLabelUri(viewLicenseLabel, package.LicenseUrl);
                titleAuthorshipPanel.Margin = new Padding(0, 3, 3, 3);
                titleAuthorshipPanel.AutoSize = true;
                titleAuthorshipPanel.Controls.Add(titleLabel);
                titleAuthorshipPanel.Controls.Add(authorshipLabel);
                packageLicenseView.Controls.Add(titleAuthorshipPanel);
                packageLicenseView.Controls.Add(viewLicenseLabel);
            }
            ResumeLayout();
        }

        static void SetLinkLabelUri(LinkLabel linkLabel, Uri uri)
        {
            linkLabel.Links[0].Description = uri != null && uri.IsAbsoluteUri ? uri.AbsoluteUri : null;
            linkLabel.Links[0].LinkData = uri;
        }

        void viewLicenseLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkUri = (Uri)e.Link.LinkData;
            if (linkUri != null)
            {
                Process.Start(linkUri.AbsoluteUri);
            }
        }
    }
}
