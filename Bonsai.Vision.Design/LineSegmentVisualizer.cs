using Bonsai;
using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Expressions;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(LineSegmentVisualizer), Target = typeof(LineSegment))]
[assembly: TypeVisualizer(typeof(LineSegmentVisualizer), Target = typeof(LineSegment[]))]

namespace Bonsai.Vision.Design
{
    public class LineSegmentVisualizer : IplImageVisualizer
    {
        const float DefaultHeight = 480;
        const int DefaultThickness = 2;
        ObjectTextVisualizer textVisualizer;
        IDisposable inputHandle;
        IplImage input;
        IplImage canvas;

        internal static void Draw(IplImage image, object value)
        {
            if (image != null)
            {
                var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
                var thickness = DefaultThickness * (int)Math.Ceiling(image.Height / DefaultHeight);
                var lines = value as IEnumerable<LineSegment>;
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        CV.Line(image, line.Start, line.End, color, thickness);
                    }
                }
                else
                {
                    var line = (LineSegment)value;
                    CV.Line(image, line.Start, line.End, color, thickness);
                }
            }
        }

        public override void Show(object value)
        {
            if (textVisualizer != null) textVisualizer.Show(value);
            else
            {
                if (input != null)
                {
                    canvas = IplImageHelper.EnsureColorCopy(canvas, input);
                    Draw(canvas, value);
                    base.Show(canvas);
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            var inputInspector = default(InspectBuilder);
            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (workflow != null && context != null)
            {
                inputInspector = workflow.Where(node => node.Value == context.Source)
                                         .Select(node => workflow.Predecessors(node)
                                                                 .Select(p => p.Value as InspectBuilder)
                                                                 .FirstOrDefault())
                                         .FirstOrDefault();
            }

            if (inputInspector != null && inputInspector.ObservableType == typeof(IplImage))
            {
                inputHandle = inputInspector.Output.Merge().Subscribe(value => input = (IplImage)value);
                base.Load(provider);
            }
            else
            {
                textVisualizer = new ObjectTextVisualizer();
                textVisualizer.Load(provider);
            }
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            if (textVisualizer != null) return textVisualizer.Visualize(source, provider);
            else return base.Visualize(source, provider);
        }

        public override void Unload()
        {
            if (canvas != null)
            {
                canvas.Close();
                canvas = null;
            }

            if (inputHandle != null)
            {
                inputHandle.Dispose();
                inputHandle = null;
            }

            if (textVisualizer != null)
            {
                textVisualizer.Unload();
                textVisualizer = null;
            }
            else base.Unload();
        }
    }
}
