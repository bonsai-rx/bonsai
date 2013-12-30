using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.Expressions;
using Bonsai.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

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

        internal static IPackage LaunchEditorBootstrapper(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            string editorPackageId,
            SemanticVersion editorPackageVersion,
            ref bool launchPackageManager)
        {
            var logger = new EventLogger();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(null, null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            var sourceRepository = sourceProvider.GetAggregate(PackageRepositoryFactory.Default, true);
            var packageManager = new LicenseAwarePackageManager(sourceRepository, editorRepositoryPath) { Logger = logger };

            var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageId);
            if (editorPackage == null)
            {
                EnableVisualStyles();
                visualStylesEnabled = true;
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
                {
                    PackageHelper.RunPackageOperation(
                        packageManager,
                        () => packageManager
                            .StartInstallPackage(editorPackageId, null)
                            .ContinueWith(task => editorPackage = task.Result));
                    if (editorPackage == null) throw new ApplicationException("Unable to install editor package.");
                    launchPackageManager = true;
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
                            .ContinueWith(task => editorPackage = task.Result));
                    launchPackageManager = true;
                }
            }

            var missingPackages = PackageHelper.GetMissingPackages(packageConfiguration.Packages, packageManager.LocalRepository).ToList();
            if (missingPackages.Count > 0)
            {
                EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageId))
                {
                    PackageHelper.RunPackageOperation(packageManager, () =>
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
            using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManagerDialog, editorPath, editorPackageId))
            {
                Application.Run(packageManagerDialog);
            }

            return Program.NormalExitCode;
        }

        internal static int LaunchWorkflowEditor(
            PackageConfiguration packageConfiguration,
            string initialFileName,
            bool start,
            Dictionary<string, string> propertyAssignments)
        {
            var elementProvider = WorkflowElementLoader.GetWorkflowElementTypes(packageConfiguration);
            var visualizerProvider = TypeVisualizerLoader.GetTypeVisualizerDictionary(packageConfiguration);

            EnableVisualStyles();
            var mainForm = new MainForm(elementProvider, visualizerProvider)
            {
                InitialFileName = initialFileName,
                StartOnLoad = start
            };
            mainForm.PropertyAssignments.AddRange(propertyAssignments);
            Application.Run(mainForm);
            return mainForm.LaunchPackageManager ? Program.RequirePackageManagerExitCode : Program.NormalExitCode;
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
    }
}
