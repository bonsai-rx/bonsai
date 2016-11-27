﻿using Bonsai.Design;
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
                DialogResult loadResult;
                var configuration = ConfigurationHelper.LoadConfiguration(out loadResult);
                if (loadResult == DialogResult.Cancel) return false;
                if (configuration == null)
                {
                    throw new InvalidOperationException("Failed to load configuration.");
                }

                var editorService = new ConfigurationEditorService(owner);
                var form = new ShaderConfigurationEditorDialog();
                form.SelectedObject = configuration;
                if (editorService.ShowDialog(form) == DialogResult.OK &&
                    loadResult == DialogResult.OK)
                {
                    ShaderManager.SaveConfiguration(configuration);
                }
            }

            return false;
        }
    }
}
