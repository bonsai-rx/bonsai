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
        static Type GetPropertyType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.PropertyDescriptor != null)
            {
                var propertyType = GetPropertyType(context.PropertyDescriptor.PropertyType);
                if (propertyType.IsPrimitive) return UITypeEditorEditStyle.DropDown;
            }

            return UITypeEditorEditStyle.None;
        }

        class PreviewSlider : Slider
        {
            protected override bool ProcessDialogKey(Keys keyData)
            {
                if (keyData == Keys.Escape) OnPreviewKeyDown(new PreviewKeyDownEventArgs(keyData));
                return base.ProcessDialogKey(keyData);
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                int? decimalPlaces = null;
                var propertyDescriptor = context.PropertyDescriptor;
                var propertyType = GetPropertyType(propertyDescriptor.PropertyType);
                var range = (RangeAttribute)propertyDescriptor.Attributes[typeof(RangeAttribute)];
                var typeCode = Type.GetTypeCode(propertyType);
                if (typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal)
                {
                    var precision = (PrecisionAttribute)propertyDescriptor.Attributes[typeof(PrecisionAttribute)];
                    if (precision != null)
                    {
                        decimalPlaces = precision.DecimalPlaces;
                    }
                }
                else decimalPlaces = 0;

                var slider = new PreviewSlider();
                slider.Minimum = (double)range.Minimum;
                slider.Maximum = (double)range.Maximum;
                slider.DecimalPlaces = decimalPlaces;

                var ownerControl = editorService as Control;
                if (ownerControl != null)
                {
                    slider.BackColor = ownerControl.BackColor;
                    slider.ForeColor = ownerControl.ForeColor;
                }

                var changed = false;
                var cancelled = false;
                slider.Converter = propertyDescriptor.Converter;
                slider.Value = Math.Max(slider.Minimum, Math.Min(Convert.ToDouble(value), slider.Maximum));
                slider.PreviewKeyDown += (sender, e) => cancelled = e.KeyCode == Keys.Escape;
                slider.ValueChanged += (sender, e) =>
                {
                    changed = true;
                    propertyDescriptor.SetValue(context.Instance, Convert.ChangeType(slider.Value, propertyType));
                };
                editorService.DropDownControl(slider);

                if (cancelled && changed) propertyDescriptor.SetValue(context.Instance, value);
                return cancelled ? value : Convert.ChangeType(slider.Value, propertyType);
            }

            return base.EditValue(context, provider, value);
        }
    }
}
