using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using OpenCV.Net;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Vision.Design
{
    public class BinaryRegionExtremesVisualizer : IplImageVisualizer
    {
        ConnectedComponent connectedComponent;
        IDisposable inputHandle;

        public override void Show(object value)
        {
            var tuple = (Tuple<CvPoint2D32f, CvPoint2D32f>)value;
            if (connectedComponent != null)
            {
                var validContour = connectedComponent.Contour != null && !connectedComponent.Contour.IsInvalid;
                var boundingBox = validContour ? connectedComponent.Contour.Rect : new CvRect(0, 0, 1, 1);
                var output = new IplImage(new CvSize(boundingBox.Width, boundingBox.Height), 8, 3);
                output.SetZero();

                if (validContour)
                {
                    DrawingHelper.DrawConnectedComponent(output, connectedComponent, new CvPoint2D32f(-boundingBox.X, -boundingBox.Y));
                    if (tuple.Item1.X > 0 && tuple.Item1.Y > 0)
                    {
                        var projectedPoint = tuple.Item1 - new CvPoint2D32f(boundingBox.X, boundingBox.Y);
                        Core.cvCircle(output, new CvPoint(projectedPoint), 3, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
                    }

                    if (tuple.Item2.X > 0 && tuple.Item2.Y > 0)
                    {
                        var projectedPoint = tuple.Item2 - new CvPoint2D32f(boundingBox.X, boundingBox.Y);
                        Core.cvCircle(output, new CvPoint(projectedPoint), 3, CvScalar.Rgb(0, 255, 0), -1, 8, 0);
                    }
                }

                base.Show(output);
            }
        }

        public override void Load(IServiceProvider provider)
        {
            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (workflow != null && context != null)
            {
                var predecessorNode = (from node in workflow
                                       let builder = node.Value
                                       where builder == context.Source
                                       select workflow.Predecessors(workflow.Predecessors(node).Single()).SingleOrDefault()).SingleOrDefault();
                if (predecessorNode != null)
                {
                    var inputInspector = (InspectBuilder)predecessorNode.Value;
                    inputHandle = inputInspector.Output.Subscribe(value => connectedComponent = (ConnectedComponent)value);
                }
            }

            base.Load(provider);
        }

        public override void Unload()
        {
            if (inputHandle != null)
            {
                inputHandle.Dispose();
                inputHandle = null;
            }
            base.Unload();
        }
    }
}
