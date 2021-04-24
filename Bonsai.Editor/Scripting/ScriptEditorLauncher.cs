using Bonsai.Editor.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Bonsai.Editor.Scripting
{
    static class ScriptEditorLauncher
    {
        const string ScriptEditor = "code";
        const string ScriptExtension = ".cs";

        public static void Launch(IWin32Window owner, string projectFileName, string scriptFile = null)
        {
            var projectDirectory = Path.GetDirectoryName(projectFileName);
            var arguments = "\"" + projectDirectory + "\"";
            if (!string.IsNullOrEmpty(scriptFile))
            {
                if (string.IsNullOrEmpty(Path.GetExtension(scriptFile)))
                {
                    scriptFile = Path.ChangeExtension(scriptFile, ScriptExtension);
                }
                arguments += " \"" + scriptFile + "\"";
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ScriptEditor,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                });
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
                    EditorDialog.OpenUri("https://code.visualstudio.com/");
                }
            }
        }
    }
}
