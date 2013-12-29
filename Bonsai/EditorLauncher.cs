using Bonsai.Configuration;
using Bonsai.Editor;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai
{
    class EditorLauncher
    {
        internal static int Run(PackageConfiguration packageConfiguration, string initialFileName, bool start, Dictionary<string, string> propertyAssignments)
        {
            var elementProvider = WorkflowElementLoader.GetWorkflowElementTypes(packageConfiguration);
            var visualizerProvider = TypeVisualizerLoader.GetTypeVisualizerDictionary(packageConfiguration);

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
