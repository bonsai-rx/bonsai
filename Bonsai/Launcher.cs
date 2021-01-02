using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;
using Bonsai.Properties;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
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
        internal static readonly NuGetFramework ProjectFramework = NuGetFramework.ParseFolder("net472");

        internal static int LaunchPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            bool updatePackages)
        {
            EditorBootstrapper.EnableVisualStyles();
            using (var packageManagerDialog = new PackageManagerDialog(ProjectFramework, editorRepositoryPath))
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog.PackageManager, editorPath, editorPackageName))
            {
                packageManagerDialog.DefaultTab = updatePackages ? PackageManagerTab.Updates : PackageManagerTab.Browse;
                if (packageManagerDialog.ShowDialog() == DialogResult.OK)
                {
                    AppResult.SetResult(packageManagerDialog.InstallPath);
                }

                return Program.NormalExitCode;
            }
        }

        internal static int LaunchWorkflowEditor(
            PackageConfiguration packageConfiguration,
            ScriptExtensions scriptExtensions,
            string editorRepositoryPath,
            string initialFileName,
            float editorScale,
            bool start,
            bool debugging,
            Dictionary<string, string> propertyAssignments)
        {
            var elementProvider = WorkflowElementLoader.GetWorkflowElementTypes(packageConfiguration);
            var visualizerProvider = TypeVisualizerLoader.GetVisualizerTypes(packageConfiguration);
            var packageManager = EditorBootstrapper.Default.CreatePackageManager(editorRepositoryPath);
            using var cancellation = new CancellationTokenSource();
            var updatesAvailable = Task.Run(async () =>
            {
                try
                {
                    var localPackages = packageManager.LocalRepository.GetLocalPackages();
                    foreach (var repository in packageManager.SourceRepositoryProvider.GetRepositories())
                    {
                        try
                        {
                            if (cancellation.IsCancellationRequested) break;
                            var updates = await repository.GetUpdatesAsync(localPackages, includePrerelease: false, cancellation.Token);
                            if (updates.Any()) return true;
                        }
                        catch { continue; }
                    }

                    return false;
                }
                catch { return false; }
            }, cancellation.Token);

            EditorBootstrapper.EnableVisualStyles();
            var scriptEnvironment = new ScriptExtensionsEnvironment(scriptExtensions);
            using var mainForm = new EditorForm(elementProvider, visualizerProvider, scriptEnvironment, editorScale);
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
                return (int)mainForm.EditorResult;
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

        internal static int LaunchWorkflowPlayer(string fileName, PackageConfiguration packageConfiguration, Dictionary<string, string> propertyAssignments)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("No workflow file was specified.");
                return Program.NormalExitCode;
            }

            var visualizerProvider = Observable.Defer(() =>
            {
                EditorBootstrapper.EnableVisualStyles();
                return TypeVisualizerLoader.GetVisualizerTypes(packageConfiguration);
            });
            WorkflowRunner.Run(fileName, propertyAssignments, visualizerProvider);
            return Program.NormalExitCode;
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

        internal static int LaunchExportPackage(PackageConfiguration packageConfiguration, string fileName, string editorFolder)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("No workflow file was specified.");
                return Program.NormalExitCode;
            }

            EditorBootstrapper.EnableVisualStyles();
            var directoryName = Path.GetDirectoryName(fileName);
            if (Path.GetFileName(directoryName) != Path.GetFileNameWithoutExtension(fileName))
            {
                MessageBox.Show(
                    string.Format(Resources.ExportPackageInvalidDirectory,
                    Path.GetFileNameWithoutExtension(fileName)),
                    typeof(Launcher).Namespace,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return Program.NormalExitCode;
            }

            Manifest manifest;
            var metadataPath = Path.ChangeExtension(fileName, NuGetConstants.ManifestExtension);
            try { manifest = PackageBuilderHelper.CreatePackageManifest(metadataPath); }
            catch (XmlException ex) { return ShowManifestReadError(metadataPath, ex.Message); }
            catch (InvalidOperationException ex)
            {
                return ShowManifestReadError(
                    metadataPath,
                    ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            bool updateDependencies;
            var builder = PackageBuilderHelper.CreateExecutablePackage(fileName, manifest, packageConfiguration, out updateDependencies);
            using (var builderDialog = new PackageBuilderDialog())
            {
                builderDialog.MetadataPath = Path.ChangeExtension(fileName, NuGetConstants.ManifestExtension);
                builderDialog.InitialDirectory = Path.Combine(editorFolder, NuGet.Constants.GalleryDirectory);
                builderDialog.SetPackageBuilder(builder);
                if (updateDependencies)
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
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, galleryDialog.PackageManager, editorPath, editorPackageName))
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
