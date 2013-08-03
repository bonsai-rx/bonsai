using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Reflection;
using System.Reactive.Linq;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    public abstract class IplImageRectangleEditor : UITypeEditor
    {
        protected IplImageRectangleEditor(RectangleSource source)
        {
            Source = source;
        }

        private RectangleSource Source { get; set; }

        protected enum RectangleSource
        {
            Input,
            Output
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var rectangle = (CvRect)value;
                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                using (var imageControl = new IplImageRectanglePicker())
                {
                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    imageControl.Rectangle = rectangle;
                    imageControl.RectangleChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Rectangle);
                    visualizerDialog.AddControl(imageControl);
                    imageControl.Canvas.DoubleClick += (sender, e) =>
                    {
                        if (imageControl.Image != null)
                        {
                            visualizerDialog.ClientSize = new Size(imageControl.Image.Width, imageControl.Image.Height);
                        }
                    };

                    var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                    if (workflow == null) return base.EditValue(context, provider, value);

                    var workflowNode = (from node in workflow
                                        let builder = node.Value as TransformBuilder
                                        where builder != null && builder.Transform == context.Instance
                                        select node)
                                        .FirstOrDefault();
                    if (workflowNode == null) return base.EditValue(context, provider, value);

                    IObservable<object> source;
                    switch (Source)
                    {
                        case RectangleSource.Input: source = ((InspectBuilder)workflow.Predecessors(workflowNode).First().Value).Output.Merge(); break;
                        case RectangleSource.Output: source = ((InspectBuilder)workflow.Successors(workflowNode).First().Value).Output.Merge(); break;
                        default: return base.EditValue(context, provider, value);
                    }

                    IDisposable subscription = null;
                    imageControl.Load += delegate { subscription = source.Subscribe(image => imageControl.Image = (IplImage)image); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);

                    return imageControl.Rectangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
