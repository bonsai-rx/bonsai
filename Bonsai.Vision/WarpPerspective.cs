using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("Source")]
    [Description("Applies a perspective transformation to the input image.")]
    public class WarpPerspective : Transform<IplImage, IplImage>
    {
        public WarpPerspective()
        {
            Flags = WarpFlags.Linear;
        }

        [Description("Coordinates of the four source quadrangle vertices in the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point2f[] Source { get; set; }

        [Description("Coordinates of the four corresponding quadrangle vertices in the output image.")]
        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point2f[] Destination { get; set; }

        [Description("Specifies interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; }

        [Description("The value to which all outlier pixels will be set to.")]
        public Scalar FillValue { get; set; }

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

                    CV.WarpPerspective(input, output, mapMatrix, Flags | WarpFlags.FillOutliers, FillValue);
                    return output;
                });
            });
        }
    }
}
