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
            Method = ContourApproximation.CHAIN_APPROX_NONE;
        }

        [Description("Specifies the contour retrieval strategy.")]
        public ContourRetrieval Mode { get; set; }

        [Description("The approximation method used to output the contours.")]
        public ContourApproximation Method { get; set; }

        [Description("The optional offset to apply to individual contour points.")]
        public CvPoint Offset { get; set; }

        [Description("The minimum area for individual contours to be accepted.")]
        public double MinArea { get; set; }

        public override IObservable<Contours> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage temp = null;
                return source.Select(input =>
                {
                    CvSeq currentContour;
                    temp = IplImageHelper.EnsureImageFormat(temp, input.Size, 8, 1);
                    Core.cvCopy(input, temp);

                    var storage = new CvMemStorage();
                    var scanner = ImgProc.cvStartFindContours(temp, storage, CvContour.HeaderSize, Mode, Method, Offset);
                    while (!(currentContour = scanner.FindNextContour()).IsInvalid)
                    {
                        if (MinArea > 0 && ImgProc.cvContourArea(currentContour, CvSlice.WholeSeq, 0) < MinArea)
                        {
                            scanner.SubstituteContour(CvSeq.Null);
                        }
                    }

                    return new Contours(scanner.EndFindContours(), input.Size);
                });
            });
        }
    }
}
