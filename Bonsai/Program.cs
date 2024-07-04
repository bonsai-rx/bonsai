using Bonsai.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace Bonsai
{
    static class Program
    {
        const string StartCommand = "--start";
        const string LibraryCommand = "--lib";
        const string PropertyCommand = "--property";
        const string LayoutCommand = "--visualizer-layout";
        const string DebugScriptCommand = "--debug-scripts";
        const string EditorScaleCommand = "--editor-scale";
        const string StartWithoutDebugging = "--start-no-debug";
        const string SuppressBootstrapCommand = "--no-boot";
        const string SuppressEditorCommand = "--no-editor";
        const string PackageManagerCommand = "--package-manager";
        const string PackageManagerUpdates = "updates";
        const string ExportPackageCommand = "--export-package";
        const string ExportImageCommand = "--export-image";
        const string ReloadEditorCommand = "--reload-editor";
        const string GalleryCommand = "--gallery";
        const string PipeCommand = "--@pipe";
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
            SystemResourcesExtensionsSupport.Initialize();

            var start = false;
            var bootstrap = true;
            var debugging = false;
            var launchEditor = true;
            var debugScripts = false;
            var editorScale = 1.0f;
            var exportImage = false;
            var updatePackages = false;
            var launchResult = default(EditorResult);
            var initialFileName = default(string);
            var imageFileName = default(string);
            var pipeHandle = default(string);
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
            parser.RegisterCommand(PipeCommand, pipeName => pipeHandle = pipeName);
            parser.RegisterCommand(ExportImageCommand, fileName => { imageFileName = fileName; exportImage = true; });
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
                using var pipeClient = pipeHandle != null ? new NamedPipeClientStream(".", pipeHandle, PipeDirection.Out) : null;
                using var pipeWriter = AppResult.OpenWrite(pipeClient);
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
                        AppResult.SetResult(EditorResult.Exit);
                        AppResult.SetResult(initialFileName);
                        AppResult.SetResult((int)launchResult);
                        return NormalExitCode;
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
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration, assemblyLock: false);
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
                    using var scriptExtensions = ScriptExtensionsProvider.CompileAssembly(Launcher.ProjectFramework, packageConfiguration, editorRepositoryPath, debugScripts);
                    ConfigurationHelper.SetAssemblyResolve(packageConfiguration);
                    if (exportImage) return Launcher.LaunchExportImage(initialFileName, imageFileName, packageConfiguration);
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
                catch (AggregateException ex)
                {
                    Console.Error.WriteLine(ex);
                    return ErrorExitCode;
                }

                var startScreen = launchEditor;
                var pipeName = Guid.NewGuid().ToString();
                args = Array.FindAll(args, arg => arg != DebugScriptCommand);
                do
                {
                    var editorArgs = new List<string>(args);
                    var workingDirectory = Environment.CurrentDirectory;
                    if (launchEditor && startScreen) launchResult = EditorResult.Exit;
                    if (launchResult == EditorResult.ExportPackage) editorArgs.AddRange(new[] { initialFileName, ExportPackageCommand });
                    else if (launchResult == EditorResult.OpenGallery) editorArgs.Add(GalleryCommand);
                    else if (launchResult == EditorResult.ManagePackages)
                    {
                        editorArgs.AddRange(updatePackages
                            ? new[] { PackageManagerCommand + ":" + PackageManagerUpdates }
                            : new[] { PackageManagerCommand });
                    }
                    else
                    {
                        if (debugScripts) editorArgs.Add(DebugScriptCommand);
                        if (launchResult == EditorResult.ReloadEditor) editorArgs.Add(ReloadEditorCommand);
                        else editorArgs.Add(SuppressBootstrapCommand);
                        if (!string.IsNullOrEmpty(initialFileName))
                        {
                            if (Directory.Exists(initialFileName))
                            {
                                workingDirectory = initialFileName;
                                initialFileName = string.Empty;
                            }
                            else
                            {
                                editorArgs.Add(initialFileName);
                                workingDirectory = Path.GetDirectoryName(initialFileName);
                            }
                        }
                    }

                    using var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
                    editorArgs = editorArgs.ConvertAll(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg);
                    editorArgs.AddRange(new[] { PipeCommand, pipeName });

                    var setupInfo = new ProcessStartInfo();
                    setupInfo.FileName = Assembly.GetEntryAssembly().Location;
                    setupInfo.Arguments = string.Join(" ", editorArgs);
                    setupInfo.WorkingDirectory = workingDirectory;
                    setupInfo.UseShellExecute = false;
                    var process = Process.Start(setupInfo);
                    pipeServer.WaitForConnection();
                    using var pipeReader = AppResult.OpenRead(pipeServer);
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                        return process.ExitCode;

                    launchResult = AppResult.GetResult<EditorResult>();
                    if (launchEditor)
                    {
                        if (launchResult == EditorResult.ReloadEditor) launchEditor = false;
                        else startScreen = launchResult != EditorResult.Exit;
                    }

                    if (launchResult != EditorResult.Exit && launchResult != EditorResult.ReloadEditor)
                    {
                        if (launchResult == EditorResult.OpenGallery ||
                            launchResult == EditorResult.ManagePackages)
                        {
                            var result = AppResult.GetResult<string>();
                            if (!string.IsNullOrEmpty(result) && File.Exists(result))
                            {
                                initialFileName = result;
                            }
                        }
                        launchResult = EditorResult.ReloadEditor;
                    }
                    else
                    {
                        var editorFlags = AppResult.GetResult<EditorFlags>();
                        debugScripts = editorFlags.HasFlag(EditorFlags.DebugScripts);
                        updatePackages = editorFlags.HasFlag(EditorFlags.UpdatesAvailable);
                        initialFileName = AppResult.GetResult<string>();
                        launchResult = (EditorResult)AppResult.GetResult<int>();
                    }
                }
                while (launchResult != EditorResult.Exit);
            }

            return NormalExitCode;
        }
    }
}
