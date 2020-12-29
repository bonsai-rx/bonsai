using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;
using NuGet.Common;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai
{
    class EditorBootstrapper : Bootstrapper
    {
        public static readonly Bootstrapper Default = new EditorBootstrapper();

        static bool visualStylesEnabled;

        public static void EnableVisualStyles()
        {
            if (!visualStylesEnabled)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                visualStylesEnabled = true;
            }
        }

        public override LicenseAwarePackageManager CreatePackageManager(string path)
        {
            var packageManager = base.CreatePackageManager(path);
            packageManager.Logger = new EventLogger();
            return packageManager;
        }

        protected override Task RunPackageOperationAsync(LicenseAwarePackageManager packageManager, Func<Task> operationFactory)
        {
            EnableVisualStyles();
            EventHandler<RequiringLicenseAcceptanceEventArgs> requiringLicenseHandler = null;
            using var dialog = new PackageOperationDialog { ShowInTaskbar = true };
            requiringLicenseHandler = (sender, e) =>
            {
                if (dialog.InvokeRequired) dialog.Invoke(requiringLicenseHandler, sender, e);
                else
                {
                    using var licenseDialog = new LicenseAcceptanceDialog(e.LicensePackages);
                    e.LicenseAccepted = licenseDialog.ShowDialog() == DialogResult.Yes;
                }
            };

            dialog.RegisterEventLogger((EventLogger)packageManager.Logger);
            packageManager.RequiringLicenseAcceptance += requiringLicenseHandler;
            try
            {
                var operation = operationFactory();
                operation.ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        if (task.Exception is AggregateException ex)
                        {
                            packageManager.Logger.LogError(ex.InnerException.Message);
                        }
                    }
                    else dialog.BeginInvoke((Action)dialog.Close);
                });
                dialog.ShowDialog();
                return operation;
            }
            finally { packageManager.RequiringLicenseAcceptance -= requiringLicenseHandler; }
        }
    }
}
