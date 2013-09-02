using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class WarpPerspective : Transform<IplImage, IplImage>
    {
        [Editor("Bonsai.Vision.Design.IplImageInputQuadrangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Point2f[] Source { get; set; }

        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Point2f[] Destination { get; set; }

        static Point2f[] InitializeQuadrangle(IplImage image)
        {
            return new[]
            {
                new Point2f(0, 0),
                new Point2f(0, image.Height),
                new Point2f(image.Width, image.Height),
                new Point2f(image.Width, 0)
            };
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Point2f[] currentSource = null;
                Point2f[] currentDestination = null;
                var mapMatrix = new Mat(3, 3, Depth.F32, 1);
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    Source = Source ?? InitializeQuadrangle(output);
                    Destination = Destination ?? InitializeQuadrangle(output);

                    if (Source != currentSource || Destination != currentDestination)
                    {
                        currentSource = Source;
                        currentDestination = Destination;
                        CV.GetPerspectiveTransform(currentSource, currentDestination, mapMatrix);
                    }

                    CV.WarpPerspective(input, output, mapMatrix, WarpFlags.Linear | WarpFlags.FillOutliers, Scalar.All(0));
                    return output;
                });
            });
        }
    }
}
