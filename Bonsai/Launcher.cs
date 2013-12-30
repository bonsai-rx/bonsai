using Bonsai.Configuration;
using Bonsai.Editor;
using Bonsai.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageId, editorPackageVersion);
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
    }
}
