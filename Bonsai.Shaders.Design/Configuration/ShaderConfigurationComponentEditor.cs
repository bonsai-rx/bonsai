using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    public class ShaderConfigurationComponentEditor : WorkflowComponentEditor
    {
        static ShaderConfigurationEditorDialog editorDialog;

        internal void EditConfiguration(ShaderWindowSettings configuration, IWin32Window owner)
        {
            if (editorDialog == null)
            {
                RefreshEventHandler editorRefreshed;
                editorDialog = new ShaderConfigurationEditorDialog();
                editorRefreshed = e => editorDialog.Close();
                TypeDescriptor.Refreshed += editorRefreshed;
                editorDialog.FormClosed += (sender, e) =>
                {
                    TypeDescriptor.Refreshed -= editorRefreshed;
                    editorDialog = null;
                };
                editorDialog.Configuration = configuration;
                editorDialog.SelectedPage = ShaderConfigurationEditorPage.Window;
                editorDialog.Show(owner);
            }
            else editorDialog.Activate();
        }

        internal void EditConfiguration(ShaderWindowSettings configuration, ShaderConfigurationEditorPage selectedPage, IWin32Window owner)
        {
            EditConfiguration(configuration, owner);
            editorDialog.SelectedPage = selectedPage;
        }

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                DialogResult loadResult;
                var configuration = ConfigurationHelper.LoadConfiguration(out loadResult);
                if (loadResult == DialogResult.Cancel) return false;
                if (configuration == null)
                {
                    throw new InvalidOperationException("Failed to load configuration.");
                }

                EditConfiguration(configuration, owner);
            }

            return false;
        }
    }
}
