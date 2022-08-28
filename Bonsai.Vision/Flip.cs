using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that flips each image in the sequence around the
    /// vertical, horizontal or both axes.
    /// </summary>
    [Description("Flips each image in the sequence around the vertical, horizontal or both axes.")]
    public class Flip : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying how to flip the image.
        /// </summary>
        [Description("Specifies how to flip the image.")]
        public FlipMode Mode { get; set; }

        /// <summary>
        /// Flips each image in an observable sequence around the vertical, horizontal
        /// or both axes.
        /// </summary>
        /// <param name="source">The sequence of images to flip.</param>
        /// <returns>The sequence of flipped images.</returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Flip(input, output, Mode);
                return output;
            });
        }
    }
}
