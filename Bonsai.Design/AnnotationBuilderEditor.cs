using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog box for editing
    /// a workflow annotation.
    /// </summary>
    public class AnnotationBuilderEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is AnnotationBuilder annotationBuilder)
                {
                    using var editorDialog = new AnnotationBuilderEditorDialog();
                    editorDialog.Annotation = annotationBuilder.Text;
                    if (editorDialog.ShowDialog(owner) == DialogResult.OK)
                    {
                        annotationBuilder.Text = editorDialog.Annotation;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
