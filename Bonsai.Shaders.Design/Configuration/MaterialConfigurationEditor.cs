using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Bonsai.Shaders.Configuration.Design
{
    class MaterialConfigurationEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        protected virtual MaterialConfigurationControl CreateEditorControl()
        {
            return new MaterialConfigurationControl();
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var configurationControl = CreateEditorControl();
                configurationControl.SelectedValue = value;
                configurationControl.SelectedValueChanged += delegate { editorService.CloseDropDown(); };
                editorService.DropDownControl(configurationControl);
                return configurationControl.SelectedValue;
            }

            return base.EditValue(context, provider, value);
        }
    }
}
