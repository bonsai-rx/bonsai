using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the contours of connected components
    /// for each binary image in the sequence.
    /// </summary>
    [Description("Finds the contours of connected components for each binary image in the sequence.")]
    public class FindContours : Transform<IplImage, Contours>
    {
        /// <summary>
        /// Gets or sets a value specifying the contour retrieval strategy.
        /// </summary>
        [Description("Specifies the contour retrieval strategy.")]
        public ContourRetrieval Mode { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the approximation method used to
        /// refine the contours.
        /// </summary>
        [Description("Specifies the approximation method used to refine the contours.")]
        public ContourApproximation Method { get; set; } = ContourApproximation.ChainApproxNone;

        /// <summary>
        /// Gets or sets the optional offset to apply to individual contour points.
        /// </summary>
        [Description("The optional offset to apply to individual contour points.")]
        public Point Offset { get; set; }

        /// <summary>
        /// Gets or sets the minimum area for individual contours to be accepted.
        /// </summary>
        [Description("The minimum area for individual contours to be accepted.")]
        public double? MinArea { get; set; }

        /// <summary>
        /// Gets or sets the maximum area for individual contours to be accepted.
        /// </summary>
        [Description("The maximum area for individual contours to be accepted.")]
        public double? MaxArea { get; set; }

        /// <summary>
        /// Finds the contours of connected components for each binary image in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of binary images for which to find the connected component
        /// contours.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Contours"/> objects representing the hierarchy
        /// of polygonal contours extracted from each binary image.
        /// </returns>
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

                    var minArea = MinArea;
                    var maxArea = MaxArea;
                    var storage = new MemStorage();
                    var scanner = CV.StartFindContours(temp, storage, Contour.HeaderSize, Mode, Method, Offset);
                    while ((currentContour = scanner.FindNextContour()) != null)
                    {
                        if (minArea.HasValue || maxArea.HasValue)
                        {
                            var contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);
                            if (contourArea < minArea || contourArea > maxArea)
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
