using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    [Description("Finds the contours of connected components in the input binary image.")]
    public class FindContours : Transform<IplImage, Contours>
    {
        public FindContours()
        {
            Method = ContourApproximation.ChainApproxNone;
        }

        [Description("Specifies the contour retrieval strategy.")]
        public ContourRetrieval Mode { get; set; }

        [Description("The approximation method used to output the contours.")]
        public ContourApproximation Method { get; set; }

        [Description("The optional offset to apply to individual contour points.")]
        public Point Offset { get; set; }

        [Description("The minimum area for individual contours to be accepted.")]
        public double? MinArea { get; set; }

        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get; set; }

        public override IObservable<Contours> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage temp = null;
                return source.Select(input =>
                {
                    Seq currentContour;
                    temp = IplImageHelper.EnsureImageFormat(temp, input.Size, IplDepth.U8, 1);
                    CV.Copy(input, temp);

                    var storage = new MemStorage();
                    var scanner = CV.StartFindContours(temp, storage, Contour.HeaderSize, Mode, Method, Offset);
                    while ((currentContour = scanner.FindNextContour()) != null)
                    {
                        if (MinArea.HasValue || MaxArea.HasValue)
                        {
                            var contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);
                            if (contourArea < MinArea || contourArea > MaxArea)
                            {
                                scanner.SubstituteContour(null);
                            }
                        }
                    }

                    return new Contours(scanner.EndFindContours(), input.Size);
                });
            });
        }
    }
}
