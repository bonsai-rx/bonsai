using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai
{
    class EditorBootstrapper : Bootstrapper
    {
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

        public EditorBootstrapper(string path)
            : base(path)
        {
            PackageManager.Logger = new EventLogger();
        }

        protected override Task RunPackageOperationAsync(Func<Task> operationFactory)
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

            dialog.RegisterEventLogger((EventLogger)PackageManager.Logger);
            PackageManager.RequiringLicenseAcceptance += requiringLicenseHandler;
            try
            {
                var operation = operationFactory();
                operation.ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        if (task.Exception is AggregateException ex)
                        {
                            PackageManager.Logger.LogError(ex.InnerException.Message);
                        }
                    }
                    else dialog.BeginInvoke((Action)dialog.Close);
                });
                dialog.ShowDialog();
                return operation;
            }
            finally { PackageManager.RequiringLicenseAcceptance -= requiringLicenseHandler; }
        }
    }
}
