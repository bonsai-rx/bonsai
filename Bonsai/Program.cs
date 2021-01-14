using Bonsai.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Bonsai
{
    static class Program
    {
        const string PathEnvironmentVariable = "PATH";
        const string StartCommand = "--start";
        const string LibraryCommand = "--lib";
        const string LayoutCommand = "--layout";
        const string PropertyCommand = "--property";
        const string DebugScriptCommand = "--debug-scripts";
        const string EditorScaleCommand = "--editor-scale";
        const string StartWithoutDebugging = "--start-no-debug";
        const string SuppressBootstrapCommand = "--no-boot";
        const string SuppressEditorCommand = "--no-editor";
        const string PackageManagerCommand = "--package-manager";
        const string PackageManagerUpdates = "updates";
        const string ExportPackageCommand = "--export-package";
        const string ReloadEditorCommand = "--reload-editor";
        const string GalleryCommand = "--gallery";
        const string EditorDomainName = "EditorDomain";
        const string RepositoryPath = "Packages";
        const string ExtensionsPath = "Extensions";
        internal const int NormalExitCode = 0;
        internal const int ErrorExitCode = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        internal static int Main(string[] args)
        {
            var start = false;
            var bootstrap = true;
            var debugging = false;
            var launchEditor = true;
            var debugScripts = false;
            var editorScale = 1.0f;
            var updatePackages = false;
            var launchResult = default(EditorResult);
            var initialFileName = default(string);
            var layoutPath = default(string);
            var libFolders = new List<string>();
            var propertyAssignments = new Dictionary<string, string>();
            var parser = new CommandLineParser();
            parser.RegisterCommand(StartCommand, () => start = debugging = true);
            parser.RegisterCommand(StartWithoutDebugging, () => start = true);
            parser.RegisterCommand(LibraryCommand, path => libFolders.Add(path));
            parser.RegisterCommand(LayoutCommand, path => layoutPath = path);
            parser.RegisterCommand(DebugScriptCommand, () => debugScripts = true);
            parser.RegisterCommand(SuppressBootstrapCommand, () => bootstrap = false);
            parser.RegisterCommand(SuppressEditorCommand, () => launchEditor = false);
            parser.RegisterCommand(ExportPackageCommand, () => { launchResult = EditorResult.ExportPackage; bootstrap = false; });
            parser.RegisterCommand(ReloadEditorCommand, () => { launchResult = EditorResult.ReloadEditor; bootstrap = false; });
            parser.RegisterCommand(GalleryCommand, () => { launchResult = EditorResult.OpenGallery; bootstrap = false; });
            parser.RegisterCommand(EditorScaleCommand, scale => editorScale = float.Parse(scale, CultureInfo.InvariantCulture));
            parser.RegisterCommand(PackageManagerCommand, option =>
            {
                launchResult = EditorResult.ManagePackages;
                updatePackages = option == PackageManagerUpdates;
                bootstrap = false;
            });
            parser.RegisterCommand(command => initialFileName = Path.GetFullPath(command));
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
            var editorPackageVersion = NuGetVersion.Parse(editorAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            var editorPackageName = new PackageIdentity(editorPackageId, editorPackageVersion);
            var editorRepositoryPath = Path.Combine(editorFolder, RepositoryPath);
            var editorExtensionsPath = Path.Combine(editorFolder, ExtensionsPath);

            var packageConfiguration = ConfigurationHelper.Load();
            if (!bootstrap)
            {
                if (launchResult == EditorResult.Exit)
                {
                    if (!string.IsNullOrEmpty(initialFileName)) launchResult = EditorResult.ReloadEditor;
                    else if (launchEditor)
                    {
                        ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                        launchResult = (EditorResult)Launcher.LaunchStartScreen(out initialFileName);
                        if (launchResult == EditorResult.ReloadEditor)
                        {
                            if (!string.IsNullOrEmpty(initialFileName) && File.Exists(initialFileName))
                            {
                                Environment.CurrentDirectory = Path.GetDirectoryName(initialFileName);
                            }
                        }
                    }
                }

                AppResult.SetResult(launchResult);
                if (launchResult == EditorResult.ExportPackage)
                {
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchExportPackage(packageConfiguration, initialFileName, editorFolder);
                }
                else if (launchResult == EditorResult.ManagePackages)
                {
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchPackageManager(
                        packageConfiguration,
                        editorRepositoryPath,
                        editorPath,
                        editorPackageName,
                        updatePackages);
                }
                else if (launchResult == EditorResult.OpenGallery)
                {
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    return Launcher.LaunchGallery(packageConfiguration, editorRepositoryPath, editorPath, editorPackageName);
                }
                else if (launchResult == EditorResult.ReloadEditor)
                {
                    if (!string.IsNullOrEmpty(initialFileName))
                    {
                        var initialPath = Path.GetDirectoryName(initialFileName);
                        var customExtensionsPath = Path.Combine(initialPath, ExtensionsPath);
                        ConfigurationHelper.RegisterPath(packageConfiguration, customExtensionsPath);
                    }

                    ConfigurationHelper.RegisterPath(packageConfiguration, editorExtensionsPath);
                    libFolders.ForEach(path => ConfigurationHelper.RegisterPath(packageConfiguration, path));
                    using var scriptExtensions = ScriptExtensionsProvider.CompileAssembly(packageConfiguration, editorRepositoryPath, debugScripts);
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    if (!launchEditor) return Launcher.LaunchWorkflowPlayer(initialFileName, layoutPath, packageConfiguration, propertyAssignments);
                    else return Launcher.LaunchWorkflowEditor(
                        packageConfiguration,
                        scriptExtensions,
                        editorRepositoryPath,
                        initialFileName,
                        editorScale,
                        start,
                        debugging,
                        propertyAssignments);
                }
            }
            else
            {
                var bootstrapper = launchEditor ? (Bootstrapper)new EditorBootstrapper(editorRepositoryPath) : new ConsoleBootstrapper(editorRepositoryPath);
                try { bootstrapper.RunAsync(Launcher.ProjectFramework, packageConfiguration, editorPath, editorPackageName).Wait(); }
                catch (AggregateException) { return ErrorExitCode; }

                var startScreen = launchEditor;
                args = Array.FindAll(args, arg => arg != DebugScriptCommand);
                do
                {
                    string[] editorArgs;
                    if (startScreen) launchResult = EditorResult.Exit;
                    if (launchResult == EditorResult.ExportPackage) editorArgs = new[] { initialFileName, ExportPackageCommand };
                    else if (launchResult == EditorResult.OpenGallery) editorArgs = new[] { GalleryCommand };
                    else if (launchResult == EditorResult.ManagePackages)
                    {
                        editorArgs = updatePackages
                            ? new[] { PackageManagerCommand + ":" + PackageManagerUpdates }
                            : new[] { PackageManagerCommand };
                    }
                    else
                    {
                        var extraArgs = new List<string>(args);
                        if (debugScripts) extraArgs.Add(DebugScriptCommand);
                        if (launchResult == EditorResult.ReloadEditor) extraArgs.Add(ReloadEditorCommand);
                        else extraArgs.Add(SuppressBootstrapCommand);
                        if (!string.IsNullOrEmpty(initialFileName)) extraArgs.Add(initialFileName);
                        editorArgs = extraArgs.ToArray();
                    }

                    var setupInfo = new AppDomainSetup();
                    setupInfo.ApplicationBase = editorFolder;
                    setupInfo.PrivateBinPath = editorFolder;
                    var currentEvidence = AppDomain.CurrentDomain.Evidence;
                    var currentPermissionSet = AppDomain.CurrentDomain.PermissionSet;
                    var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
                    setupInfo.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    setupInfo.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                    var editorDomain = AppDomain.CreateDomain(EditorDomainName, currentEvidence, setupInfo, currentPermissionSet);
                    var exitCode = (EditorResult)editorDomain.ExecuteAssembly(editorPath, editorArgs);
                    Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);

                    var editorFlags = AppResult.GetResult<EditorFlags>(editorDomain);
                    launchResult = AppResult.GetResult<EditorResult>(editorDomain);
                    if (startScreen && launchResult == EditorResult.ReloadEditor)
                    {
                        startScreen = false;
                    }

                    if (launchResult != EditorResult.Exit && launchResult != EditorResult.ReloadEditor)
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
                        launchResult = EditorResult.ReloadEditor;
                    }
                    else
                    {
                        debugScripts = editorFlags.HasFlag(EditorFlags.DebugScripts);
                        updatePackages = editorFlags.HasFlag(EditorFlags.UpdatesAvailable);
                        initialFileName = AppResult.GetResult<string>(editorDomain);
                        launchResult = exitCode;
                    }

                    AppDomain.Unload(editorDomain);
                }
                while (launchResult != EditorResult.Exit);
            }

            return NormalExitCode;
        }
    }
}
