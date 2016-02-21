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
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var configuration = ShaderManager.LoadConfiguration();
                if (configuration == null)
                {
                    throw new InvalidOperationException("Failed to load configuration.");
                }

                var editorService = new ConfigurationEditorService(owner);
                var configurationManager = new ShaderConfigurationCollectionEditor(configuration.GetType());
                configurationManager.EditValue(editorService, editorService, configuration);
                if (editorService.DialogResult == DialogResult.OK)
                {
                    ShaderManager.SaveConfiguration(configuration);
                }
            }

            return false;
        }
    }
}
