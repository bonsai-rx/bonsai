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
    public class SliderEditor : UITypeEditor
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
                int? decimalPlaces = null;
                var propertyDescriptor = context.PropertyDescriptor;
                var range = (RangeAttribute)propertyDescriptor.Attributes[typeof(RangeAttribute)];
                var typeCode = Type.GetTypeCode(propertyDescriptor.PropertyType);
                if (typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal)
                {
                    var precision = (PrecisionAttribute)propertyDescriptor.Attributes[typeof(PrecisionAttribute)];
                    if (precision != null)
                    {
                        decimalPlaces = precision.DecimalPlaces;
                    }
                }
                else decimalPlaces = 0;

                var slider = new Slider();
                slider.Minimum = (double)range.Minimum;
                slider.Maximum = (double)range.Maximum;
                slider.DecimalPlaces = decimalPlaces;
                slider.Value = Math.Max(slider.Minimum, Math.Min(Convert.ToDouble(value), slider.Maximum));
                slider.ValueChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, Convert.ChangeType(slider.Value, propertyDescriptor.PropertyType));
                editorService.DropDownControl(slider);
                return Convert.ChangeType(slider.Value, propertyDescriptor.PropertyType);
            }

            return base.EditValue(context, provider, value);
        }
    }
}
