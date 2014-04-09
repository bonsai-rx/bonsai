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
    public class NumericUpDownEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        class PreviewNumericUpDown : NumericUpDown
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
                var decimalPlaces = 0;
                var propertyDescriptor = context.PropertyDescriptor;
                var range = (RangeAttribute)propertyDescriptor.Attributes[typeof(RangeAttribute)];
                var typeCode = Type.GetTypeCode(propertyDescriptor.PropertyType);
                var precision = (PrecisionAttribute)propertyDescriptor.Attributes[typeof(PrecisionAttribute)];
                if (precision != null && (typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal))
                {
                    decimalPlaces = precision.DecimalPlaces;
                }

                var numericUpDown = new PreviewNumericUpDown();
                numericUpDown.Minimum = range.Minimum;
                numericUpDown.Maximum = range.Maximum;
                numericUpDown.DecimalPlaces = decimalPlaces;
                if (precision != null)
                {
                    numericUpDown.Increment = precision.Increment;
                }

                var changed = false;
                var cancelled = false;
                numericUpDown.Value = Math.Max(numericUpDown.Minimum, Math.Min(Convert.ToDecimal(value), numericUpDown.Maximum));
                numericUpDown.PreviewKeyDown += (sender, e) => cancelled = e.KeyCode == Keys.Escape;
                numericUpDown.ValueChanged += (sender, e) =>
                {
                    changed = true;
                    propertyDescriptor.SetValue(context.Instance, Convert.ChangeType(numericUpDown.Value, propertyDescriptor.PropertyType));
                };
                editorService.DropDownControl(numericUpDown);

                if (cancelled && changed) propertyDescriptor.SetValue(context.Instance, value);
                return cancelled ? value : Convert.ChangeType(numericUpDown.Value, propertyDescriptor.PropertyType);
            }

            return base.EditValue(context, provider, value);
        }
    }
}
