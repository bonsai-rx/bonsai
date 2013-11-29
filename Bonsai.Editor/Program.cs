using Bonsai.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var editorAssembly = typeof(Program).Assembly;
            var editorPath = editorAssembly.Location;
            var editorPackageId = editorAssembly.GetName().Name;
            var editorPackageVersion = SemanticVersion.Parse(editorAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var packageConfiguration = Configuration.ConfigurationHelper.Load();
            if (!bootstrap)
            {
                if (!string.IsNullOrEmpty(initialFileName))
                {
                    var initialPath = Path.GetDirectoryName(initialFileName);
                    Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, initialPath);
                }

                Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                if (launchPackageManager)
                {
                    var packageManagerDialog = new PackageManagerDialog(Constants.RepositoryPath);
                    using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog, editorPath, editorPackageId))
                    {
                        Application.Run(packageManagerDialog);
                    }
                }
                else Application.Run(new MainForm
                {
                    PackageConfiguration = packageConfiguration,
                    InitialFileName = initialFileName,
                    StartOnLoad = start
                });
            }
            else
            {
                var logger = new EventLogger();
                var settings = Settings.LoadDefaultSettings(null, null, null);
                var sourceProvider = new PackageSourceProvider(settings);
                var sourceRepository = sourceProvider.GetAggregate(PackageRepositoryFactory.Default, true);
                var packageManager = new PackageManager(sourceRepository, Constants.RepositoryPath) { Logger = logger };

                var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageId, editorPackageVersion);
                if (editorPackage == null)
                {
                    using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
                    {
                        PackageHelper.RunPackageOperation(
                            logger,
                            () => packageManager
                                .StartInstallPackage(editorPackageId, null)
                                .ContinueWith(task => editorPackage = task.Result));
                        launchPackageManager = true;
                    }
                }

                var missingPackages = PackageHelper.GetMissingPackages(packageConfiguration.Packages, packageManager.LocalRepository).ToList();
                if (missingPackages.Count > 0)
                {
                    using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
                    {
                        PackageHelper.RunPackageOperation(logger, () =>
                            Task.Factory.ContinueWhenAll(packageManager.StartRestorePackages(missingPackages).ToArray(), operations =>
                            {
                                foreach (var task in operations)
                                {
                                    if (task.IsFaulted || task.IsCanceled) continue;
                                    packageManager.InstallPackage(
                                        task.Result,
                                        ignoreDependencies: true,
                                        allowPrereleaseVersions: true,
                                        ignoreWalkInfo: true);
                                }

                                Task.WaitAll(operations);
                            }));
                    }
                }

                var exit = editorPackage == null;
                while (!exit)
                {
                    var editorArgs = new string[args.Length + 1];
                    Array.Copy(args, editorArgs, args.Length);

                    var setupInfo = new AppDomainSetup();
                    var editorLocation = packageConfiguration.AssemblyLocations[editorPackageId];
                    var editorBasePath = Path.GetDirectoryName(editorLocation.Location);
                    setupInfo.ApplicationBase = editorBasePath;
                    setupInfo.PrivateBinPath = editorBasePath;
                    setupInfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    var editorDomain = AppDomain.CreateDomain(EditorDomainName, null, setupInfo);
                    editorArgs[editorArgs.Length - 1] = launchPackageManager ? PackageManagerCommand : SuppressBootstrapCommand;
                    editorDomain.ExecuteAssembly(editorLocation.Location, editorArgs);

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
