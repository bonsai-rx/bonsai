using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a rich text box for editing
    /// the property value.
    /// </summary>
    public class RichTextEditor : UITypeEditor
    {
        internal static string CamelCaseToSpaces(string text)
        {
            var textBuilder = new StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]))
                {
                    textBuilder.Append(' ');
                }

                textBuilder.Append(text[i]);
            }

            return textBuilder.ToString();
        }

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
                    using var editorDialog = new RichTextEditorDialog();
                    editorDialog.Value = (string)value;
                    if (GetType() is Type editorType && editorType != typeof(RichTextEditor))
                    {
                        editorDialog.Text = CamelCaseToSpaces(editorType.Name);
                    }

                    if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                    {
                        return editorDialog.Value;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
