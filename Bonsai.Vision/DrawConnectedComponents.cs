using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Draws the set of connected components into the input image.")]
    public class DrawConnectedComponents : Transform<ConnectedComponentCollection, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
                output.SetZero();

                foreach (var component in input)
                {
                    CV.DrawContours(output, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1);
                }

                return output;
            });
        }
    }
}
