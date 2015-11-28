using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Draws the set of contours into the input image.")]
    public class DrawContours : Transform<Contours, IplImage>
    {
        public DrawContours()
        {
            MaxLevel = 1;
            Thickness = -1;
        }

        [Description("The maximum level of the contour hierarchy to draw.")]
        public int MaxLevel { get; set; }

        [Description("The thickness of the lines with which the contours are drawn.")]
        public int Thickness { get; set; }

        public override IObservable<IplImage> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
                output.SetZero();

                if (input.FirstContour != null)
                {
                    CV.DrawContours(output, input.FirstContour, Scalar.All(255), Scalar.All(0), MaxLevel, Thickness);
                }

                return output;
            });
        }

        public IObservable<IplImage> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                var rect = input.Rect;
                var output = new IplImage(new Size(rect.Width, rect.Height), IplDepth.U8, 1);
                output.SetZero();
                CV.DrawContours(output, input, Scalar.All(255), Scalar.All(0), MaxLevel, Thickness, LineFlags.Connected8, new Point(-rect.X, -rect.Y));
                return output;
            });
        }
    }
}
