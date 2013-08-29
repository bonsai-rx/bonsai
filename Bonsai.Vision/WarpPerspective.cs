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
        public CvPoint2D32f[] Source { get; set; }

        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvPoint2D32f[] Destination { get; set; }

        static CvPoint2D32f[] InitializeQuadrangle(IplImage image)
        {
            return new[]
            {
                new CvPoint2D32f(0, 0),
                new CvPoint2D32f(0, image.Height),
                new CvPoint2D32f(image.Width, image.Height),
                new CvPoint2D32f(image.Width, 0)
            };
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                CvPoint2D32f[] currentSource = null;
                CvPoint2D32f[] currentDestination = null;
                var mapMatrix = new CvMat(3, 3, CvMatDepth.CV_32F, 1);
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    Source = Source ?? InitializeQuadrangle(output);
                    Destination = Destination ?? InitializeQuadrangle(output);

                    if (Source != currentSource || Destination != currentDestination)
                    {
                        currentSource = Source;
                        currentDestination = Destination;
                        ImgProc.cvGetPerspectiveTransform(currentSource, currentDestination, mapMatrix);
                    }

                    ImgProc.cvWarpPerspective(input, output, mapMatrix, WarpFlags.Linear | WarpFlags.FillOutliers, CvScalar.All(0));
                    return output;
                });
            });
        }
    }
}
