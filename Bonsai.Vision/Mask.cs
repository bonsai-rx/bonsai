using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that sets all pixels which are not in the operation
    /// mask to a fixed value, for each image in the sequence.
    /// </summary>
    [Description("Sets all pixels which are not in the operation mask to a fixed value, for each image in the sequence.")]
    public class Mask : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        /// <summary>
        /// Gets or sets the value to which all pixels that are not in the operation
        /// mask will be set to.
        /// </summary>
        [Description("The value to which all pixels that are not in the operation mask will be set to.")]
        public Scalar FillValue { get; set; }

        /// <summary>
        /// Sets all pixels which are not in the operation mask to a fixed value,
        /// for each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs where the first value contains the images to be masked,
        /// and the second value contains the operation mask. The zero values of the mask
        /// indicate which pixels in the image should be set to the specified fill value.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where all zero pixels in the mask
        /// have been set to the specified fill value.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var mask = input.Item2;
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                output.Set(FillValue);
                CV.Copy(image, output, mask);
                return output;
            });
        }

        /// <summary>
        /// Sets all pixels which are not in the operation mask to the background image,
        /// for each foreground image in the observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of triples where the first value contains the image to be masked,
        /// the second value contains the operation mask, and the third value contains the
        /// background image. The zero values of the mask indicate which pixels in the image
        /// should be set to the corresponding pixel values in the background.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where all zero pixels in the mask
        /// have been replaced by the pixels in the background image.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var mask = input.Item2;
                var output = input.Item3.Clone();
                CV.Copy(image, output, mask);
                return output;
            });
        }
    }
}
