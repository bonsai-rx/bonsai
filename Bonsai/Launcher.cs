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
            string editorPackageId,
            SemanticVersion editorPackageVersion,
            ref EditorResult launchResult)
        {
            var packageManager = CreatePackageManager(editorRepositoryPath);
            var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageId);
            if (editorPackage == null)
            {
                EnableVisualStyles();
                visualStylesEnabled = true;
                using (var monitor = string.IsNullOrEmpty(packageConfiguration.ConfigurationFile)
                    ? new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId)
                    : (IDisposable)DisposableAction.NoOp)
                {
                    PackageHelper.RunPackageOperation(
                        packageManager,
                        () => packageManager
                            .StartInstallPackage(editorPackageId, editorPackageVersion)
                            .ContinueWith(task => editorPackage = task.Result));
                    if (editorPackage == null)
                    {
                        var assemblyName = Assembly.GetEntryAssembly().GetName();
                        MessageBox.Show("Unable to install editor package.", assemblyName.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    launchResult = EditorResult.ManagePackages;
                }
            }

            if (editorPackage.Version < editorPackageVersion)
            {
                EnableVisualStyles();
                visualStylesEnabled = true;
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
                {
                    PackageHelper.RunPackageOperation(
                        packageManager,
                        () => packageManager
                            .StartUpdatePackage(editorPackageId, editorPackageVersion)
                            .ContinueWith(task => editorPackage = task.Result),
                        operationLabel: "Updating...");
                    launchResult = EditorResult.ManagePackages;
                }
            }

            var missingPackages = GetMissingPackages(packageConfiguration.Packages, packageManager.LocalRepository).ToList();
            if (missingPackages.Count > 0)
            {
                EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
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

            return editorPackage;
        }

        internal static int LaunchPackageManager(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            string editorPackageId)
        {
            EnableVisualStyles();
            var packageManagerDialog = new PackageManagerDialog(editorRepositoryPath);
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog.PackageManager, editorPath, editorPackageId))
            {
                if (packageManagerDialog.ShowDialog() == DialogResult.OK)
                {
                    AppResult.SetResult(packageManagerDialog.InstallPath);
                }
            }

            return Program.NormalExitCode;
        }

        internal static int LaunchWorkflowEditor(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string initialFileName,
            bool start,
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
            var mainForm = new MainForm(elementProvider, visualizerProvider)
            {
                FileName = initialFileName,
                StartOnLoad = start
            };
            mainForm.PropertyAssignments.AddRange(propertyAssignments);
            updatesAvailable.Subscribe(value => mainForm.UpdatesAvailable = value);
            Application.Run(mainForm);
            AppResult.SetResult(mainForm.FileName);
            return (int)mainForm.EditorResult;
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
                var serializer = new XmlSerializer(typeof(WorkflowBuilder));
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

        internal static int LaunchExportPackage(PackageConfiguration packageConfiguration, string fileName, string editorFolder)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("No workflow file was specified.");
                return Program.NormalExitCode;
            }

            EnableVisualStyles();
            PackageBuilder builder;
            try { builder = PackageBuilderHelper.CreateWorkflowPackage(fileName, packageConfiguration); }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, typeof(Launcher).Namespace, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Program.NormalExitCode;
            }

            var builderDialog = new PackageBuilderDialog();
            builderDialog.MetadataPath = Path.ChangeExtension(fileName, global::NuGet.Constants.ManifestExtension);
            builderDialog.InitialDirectory = Path.Combine(editorFolder, NuGet.Constants.GalleryDirectory);
            builderDialog.SetPackageBuilder(builder);
            builderDialog.ShowDialog();
            return Program.NormalExitCode;
        }

        internal static int LaunchGallery(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            string editorPackageId)
        {
            EnableVisualStyles();
            using (var galleryDialog = new GalleryDialog(editorRepositoryPath))
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, galleryDialog.PackageManager, editorPath, editorPackageId))
            {
                if (galleryDialog.ShowDialog() == DialogResult.OK)
                {
                    AppResult.SetResult(galleryDialog.InstallPath);
                }
            }

            return Program.NormalExitCode;
        }
    }
}
