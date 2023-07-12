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
        static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

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
                var folderName = value as string;
                if (!string.IsNullOrEmpty(folderName))
                {
                    folderName = Path.GetFullPath(folderName);
                }
                else folderName = Environment.CurrentDirectory;

                if (IsRunningOnMono)
                {
                    using var dialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = folderName };
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        return PathConvert.GetProjectPath(dialog.SelectedPath);
                    }
                }
                else
                {
                    using var dialog = new FolderBrowserDialog { SelectedPath = folderName };
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
