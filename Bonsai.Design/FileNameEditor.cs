using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides the abstract base class for user interface editors
    /// that display a dialog box from which the user can select a file.
    /// </summary>
    public abstract class FileNameEditor : UITypeEditor
    {
        /// <summary>
        /// When overridden in a derived class, initializes the dialog
        /// box from which the user can select a file.
        /// </summary>
        /// <returns>
        /// The <see cref="FileDialog"/> object which will display the
        /// dialog box from which the user can select a file.
        /// </returns>
        protected abstract FileDialog CreateFileDialog();

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
                using (var dialog = CreateFileDialog())
                {
                    var fileName = value as string;
                    dialog.InitialDirectory = Environment.CurrentDirectory;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        dialog.FileName = fileName;
                        var directoryName = Path.GetDirectoryName(fileName);
                        if (directoryName != null && Directory.Exists(directoryName))
                        {
                            dialog.InitialDirectory = Path.GetFullPath(directoryName);
                        }
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
