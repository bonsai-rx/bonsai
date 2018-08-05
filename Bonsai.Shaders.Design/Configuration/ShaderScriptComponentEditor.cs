using Bonsai.Design;
using Bonsai.Shaders.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    class ShaderScriptComponentEditor : ShaderConfigurationComponentEditor
    {
        static GlslScriptEditorDialog editorDialog;

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (editorDialog == null)
            {
                editorDialog = new GlslScriptEditorDialog();
                editorDialog.InitialDirectory = Environment.CurrentDirectory;
                foreach (var example in GetShaderExamples())
                {
                    editorDialog.ScriptExamples.Add(example);
                }

                editorDialog.FormClosed += (sender, e) => editorDialog = null;
                editorDialog.Load += (sender, e) =>
                {
                    if (editorDialog.Owner != null)
                    {
                        editorDialog.Icon = editorDialog.Owner.Icon;
                        editorDialog.ShowIcon = true;
                    }
                };

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
