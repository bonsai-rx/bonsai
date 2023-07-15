using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Design;

namespace Bonsai.Scripting.Expressions.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog box for editing
    /// the expression script.
    /// </summary>
    public class ExpressionScriptEditor : RichTextEditor
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
            if (provider != null && !IsRunningOnMono)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    using var editorDialog = new ExpressionScriptEditorDialog();
                    editorDialog.Script = (string)value;
                    return editorService.ShowDialog(editorDialog) == DialogResult.OK
                        ? editorDialog.Script
                        : value;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
