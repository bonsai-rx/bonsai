using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bonsai.NuGet.Design.Properties;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Design
{
    static class LicenseHelper
    {
        public static void SetLicenseLinkLabel(LinkLabel linkLabel, IPackageSearchMetadata package, SourceRepository sourceRepository)
        {
            var license = package.LicenseMetadata;
            if (license != null && sourceRepository.PackageSource.IsLocal)
            {
                switch (license.Type)
                {
                    case LicenseType.File:
                        var localPackage = sourceRepository.GetLocalPackage(package.Identity);
                        SetLinkLabelData(linkLabel, license.License, license, visible: true, localPackage);
                        break;
                    case LicenseType.Expression:
                        SetLinkLabelUri(linkLabel, license.LicenseUrl);
                        break;
                    default: break;
                }
            }
            else SetLinkLabelUri(linkLabel, package.LicenseUrl);
        }

        static void SetLinkLabelUri(LinkLabel linkLabel, Uri uri)
        {
            var description = uri != null && uri.IsAbsoluteUri ? uri.AbsoluteUri : null;
            SetLinkLabelData(linkLabel, description, uri, visible: uri != null, tag: null);
        }

        static void SetLinkLabelData(
            LinkLabel linkLabel,
            string description,
            object linkData,
            bool visible,
            object tag)
        {
            linkLabel.Links[0].Description = description;
            linkLabel.Links[0].LinkData = linkData;
            linkLabel.Links[0].Tag = tag;
            linkLabel.Visible = visible;
        }

        public static async Task ShowLicenseAsync(LinkLabel.Link link, IWin32Window owner)
        {
            if (link.LinkData is Uri linkUri)
            {
                ShowExternal(linkUri.AbsoluteUri);
                return;
            }

            if (link.LinkData is LicenseMetadata license &&
                link.Tag is LocalPackageInfo localPackage)
            {
                await ShowLocalAsync(license, localPackage, owner);
            }
        }

        static async Task ShowLocalAsync(
            LicenseMetadata licenseMetadata,
            LocalPackageInfo localPackage,
            IWin32Window owner)
        {
            try
            {
                using var stream = localPackage.GetReader().GetStream(licenseMetadata.License);
                using var streamReader = new StreamReader(stream);
                var licenseText = await streamReader.ReadToEndAsync();

                using var dialog = new LicenseFileDialog();
                dialog.Text = $"{localPackage.Identity.Id} {Resources.LicenseLabel}";
                dialog.LicenseText = licenseText;
                dialog.ShowDialog(owner);
            }
            catch { } //best effort
        }

        static void ShowExternal(string uri)
        {
            var activeForm = Form.ActiveForm;
            try
            {
                if (activeForm != null) activeForm.Cursor = Cursors.AppStarting;
                if (NativeMethods.IsRunningOnMono && Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    Process.Start("xdg-open", uri);
                }
                else Process.Start(uri);
            }
            catch { } //best effort
            finally
            {
                if (activeForm != null) activeForm.Cursor = null;
            }
        }
    }
}
