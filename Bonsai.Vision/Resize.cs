using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that resizes each image in the sequence to the
    /// specified size.
    /// </summary>
    [Description("Resizes each image in the sequence to the specified size.")]
    public class Resize : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the size of the output images.
        /// </summary>
        [Description("The size of the output image.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the interpolation method used to transform
        /// individual image pixels.
        /// </summary>
        [Description("Specifies the interpolation method used to transform individual image pixels.")]
        public SubPixelInterpolation Interpolation { get; set; } = SubPixelInterpolation.Linear;

        /// <summary>
        /// Resizes each image in an observable sequence to the specified size.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to resize.
        /// </param>
        /// <returns>
        /// The sequence of resized images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (input.Size != Size)
                {
                    var output = new IplImage(Size, input.Depth, input.Channels);
                    CV.Resize(input, output, Interpolation);
                    return output;
                }
                else return input;
            });
        }
    }
}
