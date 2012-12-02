using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Vision.Design;
using Bonsai;
using Bonsai.Design;
using Bonsai.Dag;
using Bonsai.Expressions;
using OpenCV.Net;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(RoiActivityVisualizer), Target = typeof(RoiActivity))]

namespace Bonsai.Vision.Design
{
    public class RoiActivityVisualizer : IplImageVisualizer
    {
        const int RoiThickness = 1;
        static readonly CvScalar InactiveRoi = CvScalar.Rgb(255, 0, 0);
        static readonly CvScalar ActiveRoi = CvScalar.Rgb(0, 255, 0);

        IplImage input;
        IplImage canvas;
        CvFont font;

        public override void Show(object value)
        {
            var regions = (RegionActivityCollection)value;
            if (input != null)
            {
                canvas = IplImageHelper.EnsureColorCopy(canvas, input);
                for (int i = 0; i < regions.Count; i++)
                {
                    var polygon = regions[i].Roi;
                    var rectangle = regions[i].Rect;
                    var color = regions[i].Activity.Val0 > 0 ? ActiveRoi : InactiveRoi;
                    Core.cvPolyLine(canvas, new[] { polygon }, new[] { polygon.Length }, 1, 1, color, RoiThickness, 8, 0);

                    int baseline;
                    CvSize labelSize;
                    var label = i.ToString();
                    Core.cvGetTextSize(label, font, out labelSize, out baseline);
                    Core.cvPutText(canvas, i.ToString(), new CvPoint(rectangle.X + RoiThickness, rectangle.Y + labelSize.Height + RoiThickness), font, color);
                    Core.cvPutText(canvas, regions[i].Activity.Val0.ToString(), new CvPoint(rectangle.X + RoiThickness, rectangle.Y - labelSize.Height - RoiThickness), font, color);
                }

                base.Show(canvas);
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
                    inputInspector.Output.Subscribe(value => input = (IplImage)value);
                }
            }

            font = new CvFont(1);
            base.Load(provider);
        }

        public override void Unload()
        {
            if (canvas != null)
            {
                canvas.Close();
                canvas = null;
            }

            base.Unload();
        }
    }
}
