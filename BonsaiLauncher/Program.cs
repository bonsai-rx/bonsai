using System;
using System.Threading.Tasks;
using BonsaiLauncher.Properties;

namespace BonsaiLauncher;

class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
    static async Task<int> Main(string[] args)
    {
        string fileName;
        BootstrapperInfo bootstrapper;
        try
        {
            if (args.Length > 1)
                throw new ArgumentException(Resources.Error_InvalidCommandLineArguments);

            fileName = args.Length > 0 ? args[0] : string.Empty;
            if (EnvironmentSelector.TryGetLocalBootstrapper(fileName, out bootstrapper))
                await EnvironmentSelector.EnsureBootstrapperExecutable(bootstrapper);
        }
        catch (Exception ex)
        {
            CommonDialog.ShowError(ex);
            return 0;
        }

        return await EnvironmentSelector.RunProcessAsync(bootstrapper.Path, fileName);
    }
}
