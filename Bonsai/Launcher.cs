using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.Expressions;
using Bonsai.NuGet;
using Bonsai.NuGet.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Properties;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Configuration;
using System.Linq;

namespace Bonsai
{
    class Launcher : Bootstrapper
    {
        internal static int LaunchPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            bool updatePackages)
        {
            EnableVisualStyles();
            using (var packageManagerDialog = new PackageManagerDialog(editorRepositoryPath))
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
            ScriptExtensions scriptEnvironment,
            string editorRepositoryPath,
            string initialFileName,
            float editorScale,
            bool start,
            bool debugging,
            Dictionary<string, string> propertyAssignments)
        {
            var elementProvider = WorkflowElementLoader.GetWorkflowElementTypes(packageConfiguration);
            var visualizerProvider = TypeVisualizerLoader.GetTypeVisualizerDictionary(packageConfiguration);
            var packageManager = CreatePackageManager(editorRepositoryPath);
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

            EnableVisualStyles();
            using var mainForm = new MainForm(elementProvider, visualizerProvider, scriptEnvironment, editorScale);
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
                if (scriptEnvironment.DebugScripts) editorFlags |= EditorFlags.DebugScripts;
                AppResult.SetResult(editorFlags);
                AppResult.SetResult(mainForm.FileName);
                return (int)mainForm.EditorResult;
            }
            finally { cancellation.Cancel(); }
        }

        internal static int LaunchStartScreen(out string initialFileName)
        {
            EnableVisualStyles();
            using (var startScreen = new StartScreen())
            {
                Application.Run(startScreen);
                initialFileName = startScreen.FileName;
                return (int)startScreen.EditorResult;
            }
        }

        internal static int LaunchWorkflowPlayer(string fileName, Dictionary<string, string> propertyAssignments)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("No workflow file was specified.");
                return Program.NormalExitCode;
            }

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.");
            }

            WorkflowBuilder workflowBuilder;
            using (var reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();
                var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
            }

            foreach (var assignment in propertyAssignments)
            {
                workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
            }

            var workflowCompleted = new ManualResetEvent(false);
            workflowBuilder.Workflow.BuildObservable().Subscribe(
                unit => { },
                ex => { Console.WriteLine(ex); workflowCompleted.Set(); },
                () => workflowCompleted.Set());
            workflowCompleted.WaitOne();
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

            EnableVisualStyles();
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
            EnableVisualStyles();
            using (var galleryDialog = new GalleryDialog(editorRepositoryPath))
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
