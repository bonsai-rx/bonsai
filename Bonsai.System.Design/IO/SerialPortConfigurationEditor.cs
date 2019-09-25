using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace Bonsai.IO.Design
{
    [Obsolete]
    public class SerialPortConfigurationEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var configurationControl = new SerialPortConfigurationControl();
                configurationControl.SelectedValue = value;
                configurationControl.SelectedValueChanged += delegate { editorService.CloseDropDown(); };
                editorService.DropDownControl(configurationControl);
                return configurationControl.SelectedValue;
            }

            return base.EditValue(context, provider, value);
        }
    }
}
