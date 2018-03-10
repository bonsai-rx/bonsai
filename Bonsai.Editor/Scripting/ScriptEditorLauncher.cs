using Bonsai.Editor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Scripting
{
    static class ScriptEditorLauncher
    {
        const string ScriptEditor = "code";
        const string ScriptExtension = ".cs";

        public static void Launch(IWin32Window owner, string projectFileName, string scriptFile)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(scriptFile)))
            {
                scriptFile = Path.ChangeExtension(scriptFile, ScriptExtension);
            }

            var projectDirectory = Path.GetDirectoryName(projectFileName);
            try
            {
                var process = new Process();
                process.StartInfo.FileName = ScriptEditor;
                process.StartInfo.Arguments = "\"" + projectDirectory + "\" \"" + scriptFile + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
            }
            catch (Win32Exception)
            {
                var result = MessageBox.Show(
                    owner,
                    Resources.InstallScriptEditor_Question,
                    Resources.InstallScriptEditor_Question_Caption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    Process.Start("https://code.visualstudio.com/");
                }
            }
        }
    }
}
