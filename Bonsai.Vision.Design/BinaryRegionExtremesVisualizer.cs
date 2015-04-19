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
using System.Reactive.Linq;
using Point = OpenCV.Net.Point;
using Size = OpenCV.Net.Size;

namespace Bonsai.Vision.Design
{
    public class BinaryRegionExtremesVisualizer : IplImageVisualizer
    {
        ConnectedComponent connectedComponent;
        IDisposable inputHandle;

        public override void Show(object value)
        {
            var tuple = (Tuple<Point2f, Point2f>)value;
            if (connectedComponent != null)
            {
                var validContour = connectedComponent.Contour != null;
                var boundingBox = validContour ? connectedComponent.Contour.Rect : new Rect(0, 0, 1, 1);
                var output = new IplImage(new Size(boundingBox.Width, boundingBox.Height), IplDepth.U8, 3);
                output.SetZero();

                if (validContour)
                {
                    DrawingHelper.DrawConnectedComponent(output, connectedComponent, new Point2f(-boundingBox.X, -boundingBox.Y));
                    if (tuple.Item1.X > 0 && tuple.Item1.Y > 0)
                    {
                        var projectedPoint = tuple.Item1 - new Point2f(boundingBox.X, boundingBox.Y);
                        CV.Circle(output, new Point(projectedPoint), 3, Scalar.Rgb(255, 0, 0), -1);
                    }

                    if (tuple.Item2.X > 0 && tuple.Item2.Y > 0)
                    {
                        var projectedPoint = tuple.Item2 - new Point2f(boundingBox.X, boundingBox.Y);
                        CV.Circle(output, new Point(projectedPoint), 3, Scalar.Rgb(0, 255, 0), -1);
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
                var predecessorNode = workflow.Where(node => node.Value == context.Source)
                                              .Select(node => workflow.Predecessors(node).FirstOrDefault())
                                              .FirstOrDefault();
                if (predecessorNode != null)
                {
                    var inputInspector = (InspectBuilder)predecessorNode.Value;
                    inputHandle = inputInspector.Output.Merge().Subscribe(value => connectedComponent = (ConnectedComponent)value);
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
