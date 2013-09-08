using Bonsai.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    static class Program
    {
        const string StartCommand = "--start";
        const string SuppressBootstrapCommand = "--noboot";
        const string PackageManagerCommand = "--packagemanager";
        const string EditorDomainName = "EditorDomain";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var start = false;
            var bootstrap = true;
            var launchPackageManager = false;
            string initialFileName = null;
            var parser = new CommandLineParser();
            parser.RegisterCommand(StartCommand, () => start = true);
            parser.RegisterCommand(SuppressBootstrapCommand, () => bootstrap = false);
            parser.RegisterCommand(PackageManagerCommand, () => { launchPackageManager = true; bootstrap = false; });
            parser.RegisterCommand(command => initialFileName = command);
            parser.Parse(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var packageConfiguration = Configuration.ConfigurationHelper.Load();
            if (!bootstrap)
            {
                Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                if (launchPackageManager)
                {
                    var packageManagerDialog = new PackageManagerDialog(Constants.RepositoryPath);
                    using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog))
                    {
                        Application.Run(packageManagerDialog);
                    }
                }
                else Application.Run(new MainForm { InitialFileName = initialFileName, StartOnLoad = start });
            }
            else
            {
                var logger = new EventLogger();
                var settings = Settings.LoadDefaultSettings(null, null, null);
                var sourceProvider = new PackageSourceProvider(settings);
                var sourceRepository = sourceProvider.GetAggregate(PackageRepositoryFactory.Default, true);
                var packageManager = new PackageManager(sourceRepository, Constants.RepositoryPath) { Logger = logger };

                var editorPackageId = typeof(Program).Assembly.GetName().Name;
                var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageId);
                if (editorPackage == null)
                {
                    using (var dialog = new PackageOperationDialog())
                    using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager))
                    {
                        dialog.RegisterEventLogger(logger);
                        var operation = Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                ((ILogger)logger).Log(MessageLevel.Info, "Checking for latest version of {0}.", editorPackageId);
                                editorPackage = packageManager.SourceRepository.FindPackage(editorPackageId);
                                packageManager.InstallPackage(editorPackage, false, true);
                                dialog.BeginInvoke((Action)dialog.Close);
                            }
                            catch (Exception ex) { ((ILogger)logger).Log(MessageLevel.Error, ex.Message); }
                        });
                        dialog.ShowDialog();
                    }
                    launchPackageManager = true;
                }

                var exit = editorPackage == null;
                while (!exit)
                {
                    var editorArgs = new string[args.Length + 1];
                    Array.Copy(args, editorArgs, args.Length);

                    var editorLocation = packageConfiguration.AssemblyLocations[editorPackageId];
                    var editorBasePath = Path.GetDirectoryName(editorLocation.Path);
                    var editorDomain = AppDomain.CreateDomain(EditorDomainName, null, editorBasePath, editorBasePath, false);
                    editorArgs[editorArgs.Length - 1] = launchPackageManager ? PackageManagerCommand : SuppressBootstrapCommand;
                    editorDomain.ExecuteAssembly(editorLocation.Path, editorArgs);

                    if (launchPackageManager)
                    {
                        launchPackageManager = false;
                        exit = false;
                    }
                    else
                    {
                        launchPackageManager = (string)editorDomain.GetData(Constants.AppDomainLaunchPackageManagerData) == "true";
                        exit = !launchPackageManager;
                    }
                    AppDomain.Unload(editorDomain);
                }
            }
        }
    }
}
