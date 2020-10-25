using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public static class PackageOperation
    {
        static void LogException(ILogger logger, Exception exception)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    LogException(logger, innerException);
                }
            }
            else logger.Log(LogLevel.Error, exception.Message);
        }

        public static void Run(LicenseAwarePackageManager packageManager, Func<Task> operationFactory, string operationLabel = null)
        {
            EventHandler<RequiringLicenseAcceptanceEventArgs> requiringLicenseHandler = null;
            using (var dialog = new PackageOperationDialog { ShowInTaskbar = true })
            {
                if (!string.IsNullOrEmpty(operationLabel)) dialog.Text = operationLabel;
                requiringLicenseHandler = (sender, e) =>
                {
                    if (dialog.InvokeRequired) dialog.Invoke(requiringLicenseHandler, sender, e);
                    else
                    {
                        dialog.Hide();
                        using (var licenseDialog = new LicenseAcceptanceDialog(e.LicensePackages))
                        {
                            e.LicenseAccepted = licenseDialog.ShowDialog() == DialogResult.Yes;
                            if (e.LicenseAccepted)
                            {
                                dialog.Show();
                            }
                        }
                    }
                };

                dialog.RegisterEventLogger((EventLogger)packageManager.Logger);
                packageManager.RequiringLicenseAcceptance += requiringLicenseHandler;
                try
                {
                    var operation = operationFactory();
                    operation.ContinueWith(task =>
                    {
                        if (task.IsFaulted) LogException(packageManager.Logger, task.Exception);
                        else dialog.BeginInvoke((Action)dialog.Close);
                    });

                    dialog.ShowDialog();
                }
                finally { packageManager.RequiringLicenseAcceptance -= requiringLicenseHandler; }
            }
        }
    }
}
