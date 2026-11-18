using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;
using Bonsai.Properties;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Bonsai
{
    static class Launcher
    {
#if NETFRAMEWORK
        internal static readonly NuGetFramework ProjectFramework = NuGetFramework.ParseFolder("net48");
#else
        internal static readonly NuGetFramework ProjectFramework = NuGetFramework.ParseFolder("net8.0-windows7.0");
#endif

        internal static int LaunchBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            bool launchEditor)
        {
            var bootstrapper = launchEditor
                ? (Bootstrapper)new EditorBootstrapper(editorRepositoryPath)
                : new ConsoleBootstrapper(editorRepositoryPath);
            try
            {
                try
                {
                    bootstrapper.RunAsync(ProjectFramework, packageConfiguration, editorPath, editorPackageName).GetAwaiter().GetResult();
                    return Program.NormalExitCode;
                }
                catch (IncompatiblePackageException ex)
                {
                    if (!launchEditor)
                    {
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(Resources.IncompatiblePackageCommandLineMessage, editorPath);
                        return Program.ErrorExitCode;
                    }

                    return ResolveIncompatiblePackages(
                        bootstrapper,
                        ex,
                        packageConfiguration,
                        editorRepositoryPath,
                        editorPath,
                        editorPackageName)
                        ? Program.NormalExitCode
                        : Program.ErrorExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Program.ErrorExitCode;
            }
        }

        internal static int LaunchPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            bool updatePackages)
        {
            try
            {
                RestoreMissingPackages(
                    new EditorBootstrapper(editorRepositoryPath),
                    packageConfiguration,
                    editorPath,
                    editorPackageName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Program.ErrorExitCode;
            }

            ConfigurationHelper.SetAssemblyResolve(packageConfiguration, assemblyLock: false);
            ShowPackageManager(
                packageConfiguration,
                editorRepositoryPath,
                editorPath,
                editorPackageName,
                updatePackages ? PackageManagerTab.Updates : PackageManagerTab.Browse);
            return Program.NormalExitCode;
        }

        static void ShowPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            PackageManagerTab defaultTab)
        {
            EditorBootstrapper.EnableVisualStyles();
            using (var packageManagerDialog = new PackageManagerDialog(ProjectFramework, editorRepositoryPath))
            using (var monitor = new PackageConfigurationUpdater(
                ProjectFramework,
                packageConfiguration,
                packageManagerDialog.PackageManager,
                editorPath,
                editorPackageName))
            {
                packageManagerDialog.DefaultTab = defaultTab;
                if (packageManagerDialog.ShowDialog() == DialogResult.OK)
                {
                    AppResult.SetResult(packageManagerDialog.InstallPath);
                }
            }
        }

        static void RestoreMissingPackages(
            Bootstrapper bootstrapper,
            PackageConfiguration packageConfiguration,
            string editorPath,
            PackageIdentity editorPackageName)
        {
            bootstrapper.AllowIncompatiblePackages = true;
            try { bootstrapper.RunAsync(ProjectFramework, packageConfiguration, editorPath, editorPackageName).GetAwaiter().GetResult(); }
            finally { bootstrapper.AllowIncompatiblePackages = false; }
        }

        static bool ResolveIncompatiblePackages(
            Bootstrapper bootstrapper,
            IncompatiblePackageException error,
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName)
        {
            EditorBootstrapper.EnableVisualStyles();
            ConfigurationHelper.SetAssemblyResolve(packageConfiguration, assemblyLock: false);
            while (true)
            {
                var result = MessageBox.Show(
                    string.Format(Resources.IncompatiblePackageMessage, error.Message),
                    Resources.IncompatiblePackageCaption,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);
                if (result != DialogResult.OK)
                    return false;

                RestoreMissingPackages(bootstrapper, packageConfiguration, editorPath, editorPackageName);
                using (AppResult.OpenWrite(null))
                {
                    ShowPackageManager(
                        packageConfiguration,
                        editorRepositoryPath,
                        editorPath,
                        editorPackageName,
                        PackageManagerTab.Installed);
                }

                try
                {
                    bootstrapper.ValidatePackageConfigurationAsync(ProjectFramework, packageConfiguration).GetAwaiter().GetResult();
                    return true;
                }
                catch (IncompatiblePackageException ex)
                {
                    error = ex;
                }
            }
        }

        internal static int LaunchWorkflowEditor(
            PackageConfiguration packageConfiguration,
            ScriptExtensions scriptExtensions,
            string editorRepositoryPath,
            string initialFileName,
            float? editorScale,
            bool start,
            bool debugging,
            Dictionary<string, string> propertyAssignments)
        {
            var elementProvider = WorkflowElementLoader.GetWorkflowElementTypes(packageConfiguration);
            var visualizerProvider = TypeVisualizerLoader.GetVisualizerTypes(packageConfiguration);
            var editorBootstrapper = new EditorBootstrapper(editorRepositoryPath);
            var packageManager = editorBootstrapper.PackageManager;
            using var cancellation = new CancellationTokenSource();
            var updatesAvailable = Task.Run(async () =>
            {
                try
                {
                    var localSearchFilter = QueryHelper.CreateSearchFilter(includePrerelease: true, default);
                    var localPackages = await packageManager.LocalRepository.SearchAsync(
                        string.Empty,
                        localSearchFilter,
                        token: cancellation.Token);

                    var updateSearchFilter = QueryHelper.CreateSearchFilter(includePrerelease: false, Constants.LibraryPackageType);
                    foreach (var repository in packageManager.SourceRepositoryProvider.GetRepositories())
                    {
                        try
                        {
                            if (cancellation.IsCancellationRequested) break;
                            var updates = await repository.GetUpdatesAsync(localPackages, updateSearchFilter, token: cancellation.Token);
                            if (updates.Any()) return true;
                        }
                        catch { continue; }
                    }

                    return false;
                }
                catch { return false; }
            }, cancellation.Token);

            EditorBootstrapper.EnableVisualStyles();
            using var serviceContainer = new ServiceContainer();
            var scriptEnvironment = new ScriptExtensionsEnvironment(scriptExtensions);
            var documentationProvider = new DocumentationProvider(packageConfiguration, packageManager);
            serviceContainer.AddService(typeof(IScriptEnvironment), scriptEnvironment);
            serviceContainer.AddService(typeof(IDocumentationProvider), documentationProvider);
            using var mainForm = editorScale.HasValue
                ? new EditorForm(elementProvider, visualizerProvider, serviceContainer, editorScale.Value)
                : new EditorForm(elementProvider, visualizerProvider, serviceContainer);
            try
            {
                updatesAvailable.ContinueWith(
                    task => mainForm.UpdatesAvailable = !task.IsFaulted && !task.IsCanceled && task.Result,
                    cancellation.Token);
                mainForm.FileName = initialFileName;
                mainForm.PropertyAssignments.AddRange(propertyAssignments);
                mainForm.LoadAction =
                    start && debugging ? LoadAction.Start :
                    start ? LoadAction.StartWithoutDebugging :
                    LoadAction.None;
                Application.Run(mainForm);
                var editorFlags = mainForm.UpdatesAvailable ? EditorFlags.UpdatesAvailable : EditorFlags.None;
                if (scriptExtensions.DebugScripts) editorFlags |= EditorFlags.DebugScripts;
                AppResult.SetResult(editorFlags);
                AppResult.SetResult(mainForm.FileName);
                AppResult.SetResult((int)mainForm.EditorResult);
                return Program.NormalExitCode;
            }
            finally { cancellation.Cancel(); }
        }

        internal static int LaunchStartScreen(out string initialFileName)
        {
            EditorBootstrapper.EnableVisualStyles();
            using (var startScreen = new StartScreen())
            {
                Application.Run(startScreen);
                initialFileName = startScreen.FileName;
                return (int)startScreen.EditorResult;
            }
        }

        internal static int LaunchWorkflowPlayer(
            string fileName,
            string layoutPath,
            PackageConfiguration packageConfiguration,
            Dictionary<string, string> propertyAssignments,
            bool debugger)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(Resources.WorkflowFileNotSpecifiedWarning);
                return Program.NormalExitCode;
            }

            var visualizerProvider = Observable.Defer(() =>
            {
                EditorBootstrapper.EnableVisualStyles();
                return TypeVisualizerLoader.GetVisualizerTypes(packageConfiguration);
            });
            WorkflowRunner.Run(fileName, propertyAssignments, visualizerProvider, layoutPath, debugger);
            return Program.NormalExitCode;
        }

        internal static int LaunchExportImage(
            string fileName,
            string imageFileName,
            PackageConfiguration packageConfiguration)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(Resources.WorkflowFileNotSpecifiedWarning);
                return Program.NormalExitCode;
            }

            if (string.IsNullOrEmpty(imageFileName))
            {
                Console.WriteLine(Resources.ImageFileNotSpecifiedWarning);
                return Program.NormalExitCode;
            }

            WorkflowExporter.ExportImage(fileName, imageFileName);
            return Program.NormalExitCode;
        }

        internal static int LaunchWorkflowCheck(string fileName, Dictionary<string, string> propertyAssignments)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.Error.WriteLine(Resources.WorkflowFileNotSpecifiedWarning);
                return Program.ErrorExitCode;
            }

            try
            {
                return WorkflowRunner.Check(fileName, propertyAssignments)
                    ? Program.NormalExitCode
                    : Program.ErrorExitCode;
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is InvalidOperationException ||
                ex is KeyNotFoundException ||
                ex is FormatException)
            {
                var message = ex is FormatException fex && fex.InnerException is not null
                    ? $"{ex.Message} {ex.InnerException.Message}"
                    : ex.Message;
                Console.Error.WriteLine(message);
                return Program.ErrorExitCode;
            }
        }

        static int ShowManifestReadError(string path, string message)
        {
            MessageBox.Show(
                string.Format(Resources.ExportPackageManifestReadError,
                Path.GetFileName(path), message),
                typeof(Launcher).Namespace,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return Program.NormalExitCode;
        }

        internal static int LaunchExportPackage(PackageConfiguration packageConfiguration, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(Resources.WorkflowFileNotSpecifiedWarning);
                return Program.NormalExitCode;
            }

            var metadataPath = Path.ChangeExtension(fileName, NuGetConstants.ManifestExtension);
            var manifest = GalleryPackage.OpenManifest(metadataPath);
            var packageBuilder = GalleryPackage.CreatePackageBuilder(fileName, manifest, packageConfiguration);
            var packageFileName = packageBuilder.Id + "." + packageBuilder.Version + NuGetConstants.PackageExtension;
            FileUtility.Replace(sourceFile =>
            {
                using var stream = File.OpenWrite(sourceFile);
                packageBuilder.Save(stream);
            }, packageFileName);
            return Program.NormalExitCode;
        }

        internal static int LaunchExportPackageDialog(PackageConfiguration packageConfiguration, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(Resources.WorkflowFileNotSpecifiedWarning);
                return Program.NormalExitCode;
            }

            Manifest manifest;
            EditorBootstrapper.EnableVisualStyles();
            var metadataPath = Path.ChangeExtension(fileName, NuGetConstants.ManifestExtension);
            var metadataExists = File.Exists(metadataPath);
            try
            {
                manifest = metadataExists
                    ? GalleryPackage.OpenManifest(metadataPath)
                    : GalleryPackage.CreateDefaultManifest(metadataPath);
            }
            catch (XmlException ex) { return ShowManifestReadError(metadataPath, ex.Message); }
            catch (InvalidOperationException ex)
            {
                return ShowManifestReadError(
                    metadataPath,
                    ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            var builder = GalleryPackage.CreatePackageBuilder(fileName, manifest, packageConfiguration);
            using (var builderDialog = new GalleryPackageBuilderDialog())
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(fileName);
                builderDialog.MetadataPath = Path.ChangeExtension(fileName, NuGetConstants.ManifestExtension);
                builderDialog.InitialDirectory = Environment.CurrentDirectory;
                builderDialog.SetPackageBuilder(builder);
                if (!metadataExists)
                {
                    builderDialog.UpdateMetadataVersion();
                }
                builderDialog.ShowDialog();
                return Program.NormalExitCode;
            }
        }

        internal static int LaunchGallery(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName)
        {
            EditorBootstrapper.EnableVisualStyles();
            using (var galleryDialog = new GalleryDialog(ProjectFramework, editorRepositoryPath))
            using (var monitor = new PackageConfigurationUpdater(
                ProjectFramework,
                packageConfiguration,
                galleryDialog.PackageManager,
                editorPath,
                editorPackageName))
            {
                if (galleryDialog.ShowDialog() == DialogResult.OK)
                {
                    AppResult.SetResult(galleryDialog.InstallPath);
                }

                return Program.NormalExitCode;
            }
        }
    }
}
