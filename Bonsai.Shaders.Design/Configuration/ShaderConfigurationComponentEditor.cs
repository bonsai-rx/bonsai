using Bonsai.Design;
using Bonsai.Shaders.Design;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    [Obsolete]
    public class ShaderConfigurationComponentEditor : ShaderScriptComponentEditor
    {
        static ShaderConfigurationEditorDialog editorDialog;

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (editorDialog == null)
            {
                editorDialog = new ShaderConfigurationEditorDialog();
                editorDialog.FormClosed += (sender, e) => editorDialog = null;
                editorDialog.SelectedPage = ShaderConfigurationEditorPage.Window;
                foreach (var example in GetShaderExamples())
                {
                    editorDialog.ScriptExamples.Add(example);
                }
                editorDialog.Show(owner);
            }
            else
            {
                if (editorDialog.WindowState == FormWindowState.Minimized)
                {
                    editorDialog.WindowState = FormWindowState.Normal;
                }
                editorDialog.Activate();
            }
            return false;
        }
    }
}
