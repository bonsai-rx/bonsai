using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.Expressions;
using Bonsai.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using PackageReference = Bonsai.Configuration.PackageReference;
using PackageHelper = Bonsai.NuGet.PackageHelper;
using Bonsai.Properties;

namespace Bonsai
{
    class Launcher
    {
        static bool visualStylesEnabled;

        static void EnableVisualStyles()
        {
            if (!visualStylesEnabled)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                visualStylesEnabled = true;
            }
        }

        static LicenseAwarePackageManager CreatePackageManager(string path)
        {
            var logger = new EventLogger();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(null, null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            var sourceRepository = sourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, true);
            return new LicenseAwarePackageManager(sourceRepository, path) { Logger = logger };
        }

        static SemanticVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            return SemanticVersion.Parse(version);
        }

        static IEnumerable<PackageReference> GetMissingPackages(IEnumerable<PackageReference> packages, IPackageRepository repository)
        {
            return from package in packages
                   let version = ParseVersion(package.Version)
                   where !repository.Exists(package.Id, version)
                   select package;
        }

        internal static IPackage LaunchEditorBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            IPackageName editorPackageName)
        {
            const string OldExtension = ".old";
            var backupExePath = editorPath + OldExtension;
            if (File.Exists(backupExePath))
            {
                try { File.Delete(backupExePath); }
                catch { } // best effort
            }

            var packageManager = CreatePackageManager(editorRepositoryPath);
            var missingPackages = GetMissingPackages(packageConfiguration.Packages, packageManager.LocalRepository).ToList();
            if (missingPackages.Count > 0)
            {
                EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageName))
                {
                    PackageHelper.RunPackageOperation(packageManager, () =>
                        Task.Factory.ContinueWhenAll(missingPackages.Select(package =>
                        packageManager.StartRestorePackage(package.Id, ParseVersion(package.Version))).ToArray(), operations =>
                        {
                            foreach (var task in operations)
                            {
                                if (task.IsFaulted || task.IsCanceled) continue;
                                var package = task.Result;
                                if (packageManager.LocalRepository.Exists(package.Id))
                                {
                                    packageManager.UpdatePackage(
                                        package,
                                        updateDependencies: false,
                                        allowPrereleaseVersions: true);
                                }
                                else
                                {
                                    packageManager.InstallPackage(
                                        package,
                                        ignoreDependencies: true,
                                        allowPrereleaseVersions: true,
                                        ignoreWalkInfo: true);
                                }
                            }

                            Task.WaitAll(operations);
                        }));
                }
            }

            var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageName.Id);
            if (editorPackage == null || editorPackage.Version < editorPackageName.Version)
            {
                EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageName))
                {
                    PackageHelper.RunPackageOperation(
                        packageManager,
                        () => packageManager
                            .StartInstallPackage(editorPackageName.Id, editorPackageName.Version)
                            .ContinueWith(task => editorPackage = task.Result),
                        operationLabel: editorPackage != null ? "Updating..." : null);
                    if (editorPackage == null)
                    {
                        var assemblyName = Assembly.GetEntryAssembly().GetName();
                        var errorMessage = editorPackage == null ? Resources.InstallEditorPackageError : Resources.UpdateEditorPackageError;
                        MessageBox.Show(errorMessage, assemblyName.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }

            return editorPackage;
        }

        internal static string LaunchPackageBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            IPackage package)
        {
            return LaunchPackageBootstrapper(
                packageConfiguration,
                editorRepositoryPath,
                editorPath,
                null,
                package.Id,
                packageManager => packageManager.StartInstallPackage(package));
        }

        internal static string LaunchPackageBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            string targetPath,
            string packageId,
            SemanticVersion packageVersion)
        {
            return LaunchPackageBootstrapper(
                packageConfiguration,
                editorRepositoryPath,
                editorPath,
                targetPath,
                packageId,
                packageManager => packageManager.StartInstallPackage(packageId, packageVersion));
        }

        static string LaunchPackageBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            string targetPath,
            string packageId,
            Func<IPackageManager, Task> installPackage)
        {
            EnableVisualStyles();
            var installPath = string.Empty;
            var executablePackage = default(IPackage);
            var packageManager = CreatePackageManager(editorRepositoryPath);
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath))
            {
                packageManager.PackageInstalling += (sender, e) =>
                {
                    var package = e.Package;
                    if (package.Id == packageId && e.Cancel)
                    {
                        executablePackage = package;
                    }
                };

                PackageHelper.RunPackageOperation(packageManager, () => installPackage(packageManager));
            }

            if (executablePackage != null)
            {
                if (string.IsNullOrEmpty(targetPath))
                {
                    var entryPoint = executablePackage.Id + NuGet.Constants.BonsaiExtension;
                    var message = string.Format(Resources.InstallExecutablePackageWarning, executablePackage.Id);
                    var result = MessageBox.Show(message, Resources.InstallExecutablePackageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.Yes)
                    {
                        using (var dialog = new SaveFolderDialog())
                        {
                            dialog.FileName = executablePackage.Id;
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                targetPath = dialog.FileName;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(targetPath))
                {
                    var targetFileSystem = new PhysicalFileSystem(targetPath);
                    installPath = PackageHelper.InstallExecutablePackage(executablePackage, targetFileSystem);
                }
            }
            return installPath;
        }

        internal static int LaunchPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            IPackageName editorPackageName,
            bool updatePackages)
        {
            EnableVisualStyles();
            using (var packageManagerDialog = new PackageManagerDialog(editorRepositoryPath))
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog.PackageManager, editorPath, editorPackageName))
            {
                packageManagerDialog.DefaultTab = updatePackages ? PackageManagerTab.Updates : PackageManagerTab.Online;
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
            var updatesAvailable = Observable.Start(() => packageManager.SourceRepository.GetUpdates(
                packageManager.LocalRepository.GetPackages(),
                includePrerelease: false,
                includeAllVersions: false).Any())
                .Catch(Observable.Return(false));

            EnableVisualStyles();
            using (var mainForm = new MainForm(elementProvider, visualizerProvider, scriptEnvironment, editorScale))
            {
                mainForm.FileName = initialFileName;
                mainForm.PropertyAssignments.AddRange(propertyAssignments);
                mainForm.LoadAction =
                    start && debugging ? LoadAction.Start :
                    start ? LoadAction.StartWithoutDebugging :
                    LoadAction.None;
                updatesAvailable.Subscribe(value => mainForm.UpdatesAvailable = value);
                Application.Run(mainForm);
                var editorFlags = mainForm.UpdatesAvailable ? EditorFlags.UpdatesAvailable : EditorFlags.None;
                if (scriptEnvironment.DebugScripts) editorFlags |= EditorFlags.DebugScripts;
                AppResult.SetResult(editorFlags);
                AppResult.SetResult(mainForm.FileName);
                return (int)mainForm.EditorResult;
            }
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
            var metadataPath = Path.ChangeExtension(fileName, global::NuGet.Constants.ManifestExtension);
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
                builderDialog.MetadataPath = Path.ChangeExtension(fileName, global::NuGet.Constants.ManifestExtension);
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
            IPackageName editorPackageName)
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
