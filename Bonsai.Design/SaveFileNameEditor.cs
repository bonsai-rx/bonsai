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
    public class SaveFileNameEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                using (var dialog = new SaveFileDialog())
                {
                    var fileName = value as string;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        dialog.FileName = fileName;
                    }

                    dialog.Filter = "All Files (*.*)|*.*";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        return dialog.FileName;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
