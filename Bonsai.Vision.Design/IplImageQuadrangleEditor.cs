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
    public abstract class IplImageQuadrangleEditor : UITypeEditor
    {
        protected IplImageQuadrangleEditor(QuadrangleSource source)
        {
            Source = source;
        }

        private QuadrangleSource Source { get; set; }

        protected enum QuadrangleSource
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
                var quadrangle = ((Point2f[])value);
                if (quadrangle == null) return base.EditValue(context, provider, value);

                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                using (var imageControl = new IplImageQuadranglePicker())
                {
                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    Array.Copy(quadrangle, imageControl.Quadrangle, imageControl.Quadrangle.Length);
                    imageControl.QuadrangleChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Quadrangle.Clone());
                    visualizerDialog.AddControl(imageControl);
                    imageControl.Canvas.DoubleClick += (sender, e) =>
                    {
                        if (imageControl.Image != null)
                        {
                            visualizerDialog.ClientSize = new System.Drawing.Size(
                                imageControl.Image.Width,
                                imageControl.Image.Height);
                        }
                    };

                    var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                    if (workflow == null) return base.EditValue(context, provider, value);

                    var workflowNode = (from node in workflow
                                        let builder = node.Value as SelectBuilder
                                        where builder != null && builder.Selector == context.Instance
                                        select node)
                                        .FirstOrDefault();
                    if (workflowNode == null) return base.EditValue(context, provider, value);

                    IObservable<object> source;
                    switch (Source)
                    {
                        case QuadrangleSource.Input: source = ((InspectBuilder)workflow.Predecessors(workflowNode).First().Value).Output.Merge(); break;
                        case QuadrangleSource.Output: source = ((InspectBuilder)workflow.Successors(workflowNode).First().Value).Output.Merge(); break;
                        default: return base.EditValue(context, provider, value);
                    }

                    IDisposable subscription = null;
                    imageControl.Load += delegate { subscription = source.Subscribe(image => imageControl.Image = (IplImage)image); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);

                    return imageControl.Quadrangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
