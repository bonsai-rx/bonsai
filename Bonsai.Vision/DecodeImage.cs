using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that decodes an image from each byte buffer in
    /// the sequence.
    /// </summary>
    [Description("Decodes an image from each byte buffer in the sequence.")]
    public class DecodeImage : Transform<Mat, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying optional conversions applied to the
        /// decoded image.
        /// </summary>
        [Description("Specifies optional conversions applied to the decoded image.")]
        public LoadImageFlags Mode { get; set; } = LoadImageFlags.Unchanged;

        /// <summary>
        /// Decodes an image from each byte buffer in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of array-like objects storing the memory buffers
        /// to be decoded.
        /// </param>
        /// <returns>
        /// The sequence of decoded images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<Mat> source)
        {
            return source.Select(input => CV.DecodeImage(input, Mode));
        }

        /// <summary>
        /// Decodes an image from each byte array in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of array objects storing the image data to be decoded.
        /// </param>
        /// <returns>
        /// The sequence of decoded images.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<byte[]> source)
        {
            return source.Select(input =>
            {
                using (var buffer = Mat.CreateMatHeader(input))
                {
                    return CV.DecodeImage(buffer, Mode);
                }
            });
        }
    }
}
