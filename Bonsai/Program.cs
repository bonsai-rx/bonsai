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
        const string PathEnvironmentVariable = "PATH";
        const string StartCommand = "--start";
        const string LibraryCommand = "--lib";
        const string PropertyCommand = "--property";
        const string DebugScriptCommand = "--debug-scripts";
        const string SuppressBootstrapCommand = "--no-boot";
        const string SuppressEditorCommand = "--no-editor";
        const string PackageManagerCommand = "--package-manager";
        const string ExportPackageCommand = "--export-package";
        const string GalleryCommand = "--gallery";
        const string EditorDomainName = "EditorDomain";
        const string RepositoryPath = "Packages";
        const string ExtensionsPath = "Extensions";
        internal const int NormalExitCode = 0;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        internal static int Main(string[] args)
        {
            var start = false;
            var bootstrap = true;
            var launchEditor = true;
            var debugScripts = false;
            var launchResult = default(EditorResult);
            var launchPackageId = default(string);
            var launchPackageVersion = default(SemanticVersion);
            var initialFileName = default(string);
            var libFolders = new List<string>();
            var propertyAssignments = new Dictionary<string, string>();
            var parser = new CommandLineParser();
            parser.RegisterCommand(StartCommand, () => start = true);
            parser.RegisterCommand(LibraryCommand, path => libFolders.Add(path));
            parser.RegisterCommand(DebugScriptCommand, () => debugScripts = true);
            parser.RegisterCommand(SuppressBootstrapCommand, () => bootstrap = false);
            parser.RegisterCommand(SuppressEditorCommand, () => launchEditor = false);
            parser.RegisterCommand(PackageManagerCommand, () => { launchResult = EditorResult.ManagePackages; bootstrap = false; });
            parser.RegisterCommand(ExportPackageCommand, () => { launchResult = EditorResult.ExportPackage; bootstrap = false; });
            parser.RegisterCommand(GalleryCommand, option =>
            {
                if (string.IsNullOrEmpty(option))
                {
                    launchResult = EditorResult.OpenGallery;
                    bootstrap = false;
                }
                else
                {
                    var assignment = PropertyAssignment.Parse(option);
                    switch (assignment.Name)
                    {
                        case "id": launchPackageId = assignment.Value; break;
                        case "version": launchPackageVersion = SemanticVersion.Parse(assignment.Value); break;
                        default: throw new InvalidOperationException("Invalid gallery command option");
                    }
                }
            });
            parser.RegisterCommand(command => initialFileName = command);
            parser.RegisterCommand(PropertyCommand, property =>
            {
                var assignment = PropertyAssignment.Parse(property);
                propertyAssignments.Add(assignment.Name, assignment.Value);
            });
            parser.Parse(args);

            var editorAssembly = typeof(Program).Assembly;
            var editorPath = editorAssembly.Location;
            var editorFolder = Path.GetDirectoryName(editorPath);
            var editorPackageId = editorAssembly.GetName().Name;
            var editorPackageVersion = SemanticVersion.Parse(editorAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            var editorRepositoryPath = Path.Combine(editorFolder, RepositoryPath);
            var editorExtensionsPath = Path.Combine(editorFolder, ExtensionsPath);

            var packageConfiguration = Configuration.ConfigurationHelper.Load();
            if (!bootstrap)
            {
                if (launchResult == EditorResult.ExportPackage)
                {
                    Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchExportPackage(packageConfiguration, initialFileName, editorFolder);
                }
                else if (launchResult == EditorResult.ManagePackages)
                {
                    Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchPackageManager(packageConfiguration, editorRepositoryPath, editorPath, editorPackageId);
                }
                else if (launchResult == EditorResult.OpenGallery)
                {
                    Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchGallery(packageConfiguration, editorRepositoryPath, editorPath, editorPackageId);
                }
                else
                {
                    if (!string.IsNullOrEmpty(launchPackageId))
                    {
                        initialFileName = Launcher.LaunchPackageBootstrapper(
                            packageConfiguration,
                            editorRepositoryPath,
                            editorPath,
                            initialFileName,
                            launchPackageId,
                            launchPackageVersion);
                    }
                    else if (Path.GetExtension(initialFileName) == Constants.PackageExtension)
                    {
                        var package = new OptimizedZipPackage(initialFileName);
                        initialFileName = Launcher.LaunchPackageBootstrapper(
                            packageConfiguration,
                            editorRepositoryPath,
                            editorPath,
                            package);
                    }

                    if (!string.IsNullOrEmpty(initialFileName))
                    {
                        var initialPath = Path.GetDirectoryName(initialFileName);
                        var customExtensionsPath = Path.Combine(initialPath, ExtensionsPath);
                        Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, customExtensionsPath);
                    }

                    Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, editorExtensionsPath);
                    libFolders.ForEach(path => Configuration.ConfigurationHelper.RegisterPath(packageConfiguration, path));
                    using (var scriptEnvironment = ScriptExtensionsProvider.CompileAssembly(packageConfiguration, editorRepositoryPath, debugScripts))
                    {
                        Configuration.ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                        if (!launchEditor) return Launcher.LaunchWorkflowPlayer(initialFileName, propertyAssignments);
                        else return Launcher.LaunchWorkflowEditor(
                            packageConfiguration,
                            scriptEnvironment,
                            editorRepositoryPath,
                            initialFileName,
                            start,
                            propertyAssignments);
                    }
                }
            }
            else
            {
                args = Array.FindAll(args, arg => arg != DebugScriptCommand);
                var editorPackage = Launcher.LaunchEditorBootstrapper(
                    packageConfiguration,
                    editorRepositoryPath,
                    editorPath,
                    editorPackageId,
                    editorPackageVersion,
                    ref launchResult);
                var exit = editorPackage == null;
                while (!exit)
                {
                    string[] editorArgs;
                    if (launchResult == EditorResult.ExportPackage) editorArgs = new[] { initialFileName, ExportPackageCommand };
                    else if (launchResult == EditorResult.ManagePackages) editorArgs = new[] { PackageManagerCommand };
                    else if (launchResult == EditorResult.OpenGallery) editorArgs = new[] { GalleryCommand };
                    else
                    {
                        var extraArgs = new List<string>(args);
                        extraArgs.Add(SuppressBootstrapCommand);
                        if (debugScripts) extraArgs.Add(DebugScriptCommand);
                        if (!string.IsNullOrEmpty(initialFileName)) extraArgs.Add(initialFileName);
                        editorArgs = extraArgs.ToArray();
                    }

                    var setupInfo = new AppDomainSetup();
                    setupInfo.ApplicationBase = editorFolder;
                    setupInfo.PrivateBinPath = editorFolder;
                    var currentEvidence = AppDomain.CurrentDomain.Evidence;
                    var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
                    setupInfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    setupInfo.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                    var editorDomain = AppDomain.CreateDomain(EditorDomainName, currentEvidence, setupInfo);
                    var exitCode = (EditorResult)editorDomain.ExecuteAssembly(editorPath, editorArgs);
                    Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);

                    if (launchResult != EditorResult.Exit)
                    {
                        if (launchResult == EditorResult.OpenGallery ||
                            launchResult == EditorResult.ManagePackages)
                        {
                            var result = AppResult.GetResult<string>(editorDomain);
                            if (!string.IsNullOrEmpty(result) && File.Exists(result))
                            {
                                initialFileName = result;
                                Environment.CurrentDirectory = Path.GetDirectoryName(initialFileName);
                            }
                        }
                        launchResult = EditorResult.Exit;
                    }
                    else if (exitCode == EditorResult.Exit) exit = true;
                    else
                    {
                        debugScripts = AppResult.GetResult<bool>(editorDomain);
                        initialFileName = AppResult.GetResult<string>(editorDomain);
                        launchResult = exitCode == EditorResult.ReloadEditor ? EditorResult.Exit : exitCode;
                    }

                    AppDomain.Unload(editorDomain);
                }
            }

            return NormalExitCode;
        }
    }
}
