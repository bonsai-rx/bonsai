using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Expressions;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    class PolarTransformEditor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (workflow != null && editorState != null && editorState.WorkflowRunning)
                {
                    var polarTransform = (PolarTransform)component;
                    using (var editorForm = new TypeVisualizerDialog())
                    {
                        var imageControl = new ImageCirclePicker();
                        imageControl.Dock = DockStyle.Fill;
                        imageControl.Center = polarTransform.Center;
                        imageControl.Radius = polarTransform.Magnitude;
                        imageControl.CircleChanged += (sender, e) =>
                        {
                            polarTransform.Center = imageControl.Center;
                            polarTransform.Magnitude = imageControl.Radius;
                        };
                        editorForm.Text = string.Format("{0} Center", component.GetType().Name);
                        editorForm.AddControl(imageControl);
                        imageControl.Canvas.DoubleClick += (sender, e) =>
                        {
                            if (imageControl.Image != null)
                            {
                                editorForm.ClientSize = new System.Drawing.Size(
                                    imageControl.Image.Width,
                                    imageControl.Image.Height);
                            }
                        };

                        var workflowNode = workflow.FirstOrDefault(node => ExpressionBuilder.GetWorkflowElement(node.Value) == component);
                        if (workflowNode == null)
                        {
                            throw new InvalidOperationException(string.Format("'{0}' does not support this combinator type.", GetType()));
                        }

                        IDisposable subscription = null;
                        var source = ((InspectBuilder)workflow.Predecessors(workflowNode).First().Value).Output.Merge();
                        imageControl.Load += delegate { subscription = source.Subscribe(image => imageControl.Image = (IplImage)image); };
                        imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                        editorForm.ShowDialog(owner);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
