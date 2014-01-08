using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;
using System.Windows.Forms;
using System.Drawing;

[assembly: TypeVisualizer(typeof(ContoursVisualizer), Target = typeof(Contours))]

namespace Bonsai.Vision.Design
{
    public class ContoursVisualizer : IplImageVisualizer
    {
        int thickness;

        public override void Show(object value)
        {
            var contours = (Contours)value;
            var output = new IplImage(contours.ImageSize, IplDepth.U8, 1);
            output.SetZero();

            if (contours.FirstContour != null)
            {
                CV.DrawContours(output, contours.FirstContour, Scalar.All(255), Scalar.All(128), 2, thickness);
            }

            base.Show(output);
        }

        public override void Load(IServiceProvider provider)
        {
            thickness = -1;
            base.Load(provider);
            StatusStripEnabled = false;
            VisualizerCanvas.Canvas.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    thickness *= -1;
                }
            };
        }
    }
}
