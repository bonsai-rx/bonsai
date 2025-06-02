using System;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;

namespace Bonsai
{
    internal class EditorEnvironmentBootstrapper : EnvironmentBootstrapper
    {
        public EditorEnvironmentBootstrapper()
        {
            Logger = new EventLogger();
        }

        protected override IProgressBar GetProgressBar()
        {
            return new DummyProgressBar();
        }

        protected override Task RunBootstrapperOperationAsync(BootstrapperInfo bootstrapper, CancellationToken cancellationToken)
        {
            EditorBootstrapper.EnableVisualStyles();
            using var dialog = new PackageOperationDialog { ShowInTaskbar = true };
            dialog.RegisterEventLogger((EventLogger)Logger);

            var operation = base.RunBootstrapperOperationAsync(bootstrapper, cancellationToken);
            operation.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (task.Exception is AggregateException ex)
                    {
                        Logger.LogError(ex.InnerException.Message);
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
