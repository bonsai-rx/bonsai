using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a browser dialog box
    /// from which the user can select a folder.
    /// </summary>
    public class FolderNameEditor : UITypeEditor
    {
        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    var folderName = value as string;
                    if (!string.IsNullOrEmpty(folderName))
                    {
                        dialog.SelectedPath = Path.GetFullPath(folderName);
                    }
                    else dialog.SelectedPath = Environment.CurrentDirectory;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        return PathConvert.GetProjectPath(dialog.SelectedPath);
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
