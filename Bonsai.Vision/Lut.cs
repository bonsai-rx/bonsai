using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that performs a look-up table transformation on all
    /// pixels of each image in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Performs a look-up table transformation on all pixels of each image in the sequence.")]
    public class Lut
    {
        /// <summary>
        /// Performs a look-up table transformation on all pixels of each image in an
        /// observable sequence, where each image is paired with a look-up table array.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects used as a look-up table.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs where the first value contains the images to be transformed,
        /// and the second value contains the look-up table array. The values of each image
        /// pixel are used as indices into the look-up table array to retrieve the result
        /// of the transformation.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects storing transformed image pixels.
        /// </returns>
        public IObservable<IplImage> Process<TArray>(IObservable<Tuple<IplImage, TArray>> source) where TArray : Arr
        {
            return source.Select(input =>
            {
                var image = input.Item1;
                var lut = input.Item2;
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                CV.LUT(image, output, lut);
                return output;
            });
        }
    }
}
