using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class DrawContours : Transform<Contours, IplImage>
    {
        public DrawContours()
        {
            MaxLevel = 1;
            Thickness = -1;
        }

        public int MaxLevel { get; set; }

        public int Thickness { get; set; }

        public override IObservable<IplImage> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
                output.SetZero();

                if (!input.FirstContour.IsInvalid)
                {
                    CV.DrawContours(output, input.FirstContour, Scalar.All(255), Scalar.All(0), MaxLevel, Thickness);
                }

                return output;
            });
        }
    }
}
