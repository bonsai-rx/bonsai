using Bonsai.NuGet.Design.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public partial class LicenseAcceptanceDialog : Form
    {
        public LicenseAcceptanceDialog(IEnumerable<RequiringLicenseAcceptancePackageInfo> licensePackages)
        {
            if (licensePackages == null)
            {
                throw new ArgumentNullException(nameof(licensePackages));
            }

            InitializeComponent();
            SuspendLayout();
            var bold = new Font(Font, FontStyle.Bold);
            foreach (var packageInfo in licensePackages)
            {
                var package = packageInfo.Package;
                var titleAuthorshipPanel = new FlowLayoutPanel();
                var titleLabel = new Label();
                var authorshipLabel = new Label();
                var viewLicenseLabel = new LinkLabel();
                viewLicenseLabel.AutoSize = true;
                viewLicenseLabel.LinkClicked += viewLicenseLabel_LinkClicked;

                titleLabel.Font = bold;
                titleLabel.AutoSize = true;
                titleLabel.Text = package.Identity.Id;
                authorshipLabel.AutoSize = true;
                var authorsText = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, package.Authors);
                authorshipLabel.Text = string.Format(Resources.LicenseAuthorshipLabel, authorsText);
                viewLicenseLabel.Text = Resources.LicenseViewLicenseLabel;
                LicenseHelper.SetLicenseLinkLabel(viewLicenseLabel, package, packageInfo.SourceRepository);
                titleAuthorshipPanel.Margin = new Padding(0, 3, 3, 3);
                titleAuthorshipPanel.AutoSize = true;
                titleAuthorshipPanel.Controls.Add(titleLabel);
                titleAuthorshipPanel.Controls.Add(authorshipLabel);
                packageLicenseView.Controls.Add(titleAuthorshipPanel);
                packageLicenseView.Controls.Add(viewLicenseLabel);
            }
            ResumeLayout();
        }

        async void viewLicenseLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            await LicenseHelper.ShowLicenseAsync(e.Link, this);
        }
    }
}
