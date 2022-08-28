using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that crops a rectangular subregion of each image
    /// in the sequence, without copying.
    /// </summary>
    [DefaultProperty(nameof(RegionOfInterest))]
    [Description("Crops a rectangular subregion of each image in the sequence, without copying.")]
    public class Crop : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets a rectangle specifying the region of interest inside the image.
        /// </summary>
        [Description("Specifies the region of interest inside the image.")]
        [Editor("Bonsai.Vision.Design.IplImageRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

        /// <summary>
        /// Crops a subregion of each image in an observable sequence.
        /// </summary>
        /// <param name="source">The sequence of images to crop.</param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each new image
        /// contains the extracted subregion of the original image.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var rect = RegionOfInterest;
                if (rect.Width > 0 && rect.Height > 0)
                {
                    return input.GetSubRect(rect);
                }

                return input;
            });
        }
    }
}
