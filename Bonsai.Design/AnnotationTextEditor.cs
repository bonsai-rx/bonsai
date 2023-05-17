using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog box for editing
    /// the annotation text.
    /// </summary>
    public class AnnotationTextEditor : UITypeEditor
    {
        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    using var editorDialog = new AnnotationBuilderEditorDialog();
                    editorDialog.Annotation = value as string;
                    if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                    {
                        return editorDialog.Annotation;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
