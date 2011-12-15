using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class TrackBarEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var propertyDescriptor = context.PropertyDescriptor;
                var range = (RangeAttribute)propertyDescriptor.Attributes[typeof(RangeAttribute)];

                var trackBar = new TrackBar();
                trackBar.Minimum = Convert.ToInt32(range.Minimum < int.MinValue ? int.MinValue : range.Minimum);
                trackBar.Maximum = Convert.ToInt32(range.Maximum > int.MaxValue ? int.MaxValue : range.Maximum);
                trackBar.Value = Convert.ToInt32(value);
                trackBar.ValueChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, Convert.ChangeType(trackBar.Value, propertyDescriptor.PropertyType));
                editorService.DropDownControl(trackBar);
                return Convert.ChangeType(trackBar.Value, propertyDescriptor.PropertyType);
            }

            return base.EditValue(context, provider, value);
        }
    }
}
