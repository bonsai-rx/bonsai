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
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(BinaryRegionExtremesMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, BinaryRegionExtremesVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class BinaryRegionExtremesMashupVisualizer : DialogTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var extremes = (Tuple<CvPoint2D32f, CvPoint2D32f>)value;
            Core.cvCircle(image, new CvPoint(extremes.Item1), 3, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
            Core.cvCircle(image, new CvPoint(extremes.Item2), 3, CvScalar.Rgb(0, 255, 0), -1, 8, 0);
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
