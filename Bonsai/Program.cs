using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bonsai
{
    static class Program
    {
        const string StartCommand = "--start";
        const string LibraryCommand = "--lib";
        const string PropertyCommand = "-p";
        const string PropertyAssignmentSeparator = "=";
        const string SuppressBootstrapCommand = "--noboot";
        const string SuppressEditorCommand = "--noeditor";
        const string PackageManagerCommand = "--packagemanager";
        const string EditorDomainName = "EditorDomain";
        const string RepositoryPath = "Packages";
        internal const int NormalExitCode = 0;
        internal const int RequirePackageManagerExitCode = 1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static int Main(string[] args)
        {
            var start = false;
            var bootstrap = true;
            var launchEditor = true;
            var launchPackageManager = false;
            string initialFileName = null;
            var libFolders = new List<string>();
            var propertyAssignments = new Dictionary<string, string>();
            var parser = new CommandLineParser();
            parser.RegisterCommand(StartCommand, () => start = true);
            parser.RegisterCommand(LibraryCommand, path => libFolders.Add(path));
            parser.RegisterCommand(SuppressBootstrapCommand, () => bootstrap = false);
            parser.RegisterCommand(SuppressEditorCommand, () => launchEditor = false);
            parser.RegisterCommand(PackageManagerCommand, () => { launchPackageManager = true; bootstrap = false; });
            parser.RegisterCommand(command => initialFileName = command);
            parser.RegisterCommand(PropertyCommand, property =>
            {
                var assignment = property.Split(new[] { PropertyAssignmentSeparator }, 2, StringSplitOptions.None);
                if (assignment.Length == 2)
                {
                    propertyAssignments.Add(assignment[0], assignment[1]);
                }
            });
            parser.Parse(args);

            var editorAssembly = typeof(Program).Assembly;
            var editorPath = editorAssembly.Location;
            var editorFolder = Path.GetDirectoryName(editorPath);
            var editorPackageId = editorAssembly.GetName().Name;
            var editorPackageVersion = SemanticVersion.Parse(editorAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            var editorRepositoryPath = Path.Combine(editorFolder, RepositoryPath);

            var packageConfiguration = Configuration.ConfigurationHelper.Load();
            if (!bootstrap)
            {
                if (launchPackageManager)
                {
                    Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchPackageManager(packageConfiguration, editorRepositoryPath, editorPath, editorPackageId);
                }
                else
                {
                    if (!string.IsNullOrEmpty(initialFileName))
                    {
                        var initialPath = Path.GetDirectoryName(initialFileName);
                        Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, initialPath);
                    }

                    libFolders.ForEach(path => Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, path));
                    Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    if (!launchEditor) Launcher.LaunchWorkflowPlayer(initialFileName, propertyAssignments);
                    else return Launcher.LaunchWorkflowEditor(packageConfiguration, initialFileName, start, propertyAssignments);
                }
            }
            else
            {
                var editorPackage = Launcher.LaunchEditorBootstrapper(
                    packageConfiguration,
                    editorRepositoryPath,
                    editorPath,
                    editorPackageId,
                    editorPackageVersion,
                    ref launchPackageManager);
                var exit = editorPackage == null;
                while (!exit)
                {
                    var editorArgs = new string[args.Length + 1];
                    editorArgs[editorArgs.Length - 1] = launchPackageManager ? PackageManagerCommand : SuppressBootstrapCommand;
                    Array.Copy(args, editorArgs, args.Length);

                    var setupInfo = new AppDomainSetup();
                    setupInfo.ApplicationBase = editorFolder;
                    setupInfo.PrivateBinPath = editorFolder;
                    var currentEvidence = AppDomain.CurrentDomain.Evidence;
                    setupInfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    setupInfo.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                    var editorDomain = AppDomain.CreateDomain(EditorDomainName, currentEvidence, setupInfo);
                    var exitCode = editorDomain.ExecuteAssembly(editorPath, editorArgs);

                    if (launchPackageManager)
                    {
                        launchPackageManager = false;
                        exit = false;
                    }
                    else
                    {
                        launchPackageManager = exitCode == RequirePackageManagerExitCode;
                        exit = !launchPackageManager;
                    }

                    AppDomain.Unload(editorDomain);
                }
            }

            return NormalExitCode;
        }
    }
}
