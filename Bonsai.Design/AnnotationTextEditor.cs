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
    public class AnnotationTextEditor : RichTextEditor
    {
        internal static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null && !IsRunningOnMono)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    using var editorDialog = new AnnotationBuilderEditorDialog();
                    editorDialog.Annotation = (string)value;
                    return editorService.ShowDialog(editorDialog) == DialogResult.OK
                        ? editorDialog.Annotation
                        : value;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
