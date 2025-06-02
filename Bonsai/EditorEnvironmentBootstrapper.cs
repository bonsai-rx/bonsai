using System;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;

namespace Bonsai
{
    /// <summary>
    /// In this implementation we juggle multiple competing requirements:
    ///   1. If the bootstrapper is already initialized, or if it can be resolved successfully locally,
    ///      we do not want to show a dialog.
    ///   2. If there is an error during local validation or initialization (e.g. checksum mismatch
    ///      or unexpected version), we want to show this error in a blocking dialog.
    ///   3. If the bootstrapper cannot be resolved locally, we want to show the progress bar dialog
    ///      with information while the download is progressing.
    ///   4. If the download fails for any reason, we also want to show the dialog.
    /// 
    /// These could very likely be resolved much better than the logic below, but for now the main goal
    /// was to isolate and contain the complexity.
    /// 
    /// RunEnvironmentOperationAsync is a dialog launcher coupled to an arbitrary Task which will handle
    /// failures by blocking the dialog. We call it on demand either when there is a validation exception
    /// or a remote download.
    /// </summary>
    internal class EditorEnvironmentBootstrapper : IEnvironmentBootstrapper
    {
        public async Task<bool> EnsureBoostrapperExecutableAsync(BootstrapperInfo bootstrapper, CancellationToken cancellationToken = default)
        {
            var logger = new EventLogger();
            try
            {
                if (EnvironmentSelector.TryInitializeLocalBootstrapper(bootstrapper) is null)
                    await RunEnvironmentOperationAsync(() => EnvironmentSelector.DownloadBootstrapperExecutableAsync(
                        bootstrapper,
                        logger,
                        () => new DummyProgressBar(),
                        cancellationToken),
                        logger,
                        cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                await RunEnvironmentOperationAsync(() => Task.FromException(ex), logger, cancellationToken);
                return false;
            }
        }

        static Task RunEnvironmentOperationAsync(
            Func<Task> operationFactory,
            EventLogger logger,
            CancellationToken cancellationToken)
        {
            EditorBootstrapper.EnableVisualStyles();
            using var dialog = new PackageOperationDialog { ShowInTaskbar = true };
            dialog.RegisterEventLogger(logger);

            var operation = operationFactory();
            operation.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                    {
                        logger.LogError(ex.InnerException.Message);
                    }
                }
                else dialog.BeginInvoke(dialog.Close);
            }, cancellationToken);
            dialog.ShowDialog();
            return operation;
        }

        class DummyProgressBar : IProgressBar
        {
            public void Report(int value)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
