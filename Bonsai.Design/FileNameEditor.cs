using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;

namespace Bonsai.Design
{
    public abstract class FileNameEditor : UITypeEditor
    {
        protected abstract FileDialog CreateFileDialog();

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                using (var dialog = CreateFileDialog())
                {
                    var fileName = value as string;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        dialog.FileName = fileName;
                    }

                    var filterAttribute = (FileNameFilterAttribute)context.PropertyDescriptor.Attributes[typeof(FileNameFilterAttribute)];
                    var filter = filterAttribute != null ? filterAttribute.Filter : "All Files|*.*";
                    dialog.Filter = filter;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        return PathConvert.GetProjectPath(dialog.FileName);
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
