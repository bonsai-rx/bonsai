using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Bonsai.Shaders.Configuration.Design
{
    class ShaderConfigurationControl : ConfigurationDropDown
    {
        public ShaderConfigurationControl(IServiceProvider provider)
            : base(provider)
        {
            Text = "Manage Shaders";
        }

        protected override IEnumerable<string> GetConfigurationNames()
        {
            return ShaderManager.LoadConfiguration().Shaders.Select(configuration => configuration.Name);
        }

        protected override object LoadConfiguration()
        {
            return null;
        }

        protected override void SaveConfiguration(object configuration)
        {
        }

        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return new ShaderWindowEditor
            {
                SelectedPage = ShaderConfigurationEditorPage.Shaders
            };
        }

        protected class ShaderWindowEditor : UITypeEditor
        {
            readonly ShaderConfigurationComponentEditor editor;

            public ShaderWindowEditor()
            {
                editor = new ShaderConfigurationComponentEditor();
            }

            public ShaderConfigurationEditorPage SelectedPage { get; set; }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var editorService = (IUIService)provider.GetService(typeof(IUIService));
                if (editorService != null)
                {
                    var owner = editorService.GetDialogOwnerWindow();
                    editor.EditConfiguration(SelectedPage, owner);
                    return value;
                }

                return base.EditValue(context, provider, value);
            }
        }
    }
}
