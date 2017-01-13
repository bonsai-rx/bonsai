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
            return ShaderManager.LoadConfiguration();
        }

        protected override void SaveConfiguration(object configuration)
        {
            var shaderConfiguration = configuration as ShaderWindowSettings;
            if (shaderConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ShaderManager.SaveConfiguration(shaderConfiguration);
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
            public ShaderConfigurationEditorPage SelectedPage { get; set; }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    var form = new ShaderConfigurationEditorDialog();
                    form.SelectedPage = SelectedPage;
                    form.SelectedObject = value as ShaderWindowSettings;
                    editorService.ShowDialog(form);
                    return value;
                }

                return base.EditValue(context, provider, value);
            }
        }
    }
}
