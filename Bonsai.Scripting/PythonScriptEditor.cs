using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace Bonsai.Scripting
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog box for editing
    /// the Python script.
    /// </summary>
    public class PythonScriptEditor : UITypeEditor
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
            if (editorService != null)
            {
                var script = value as string;
                var editorDialog = new PythonScriptEditorDialog();
                editorDialog.Script = script;
                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return editorDialog.Script;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
