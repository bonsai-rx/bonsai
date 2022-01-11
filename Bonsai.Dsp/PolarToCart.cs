using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes the cartesian coordinates of 2D vectors
    /// represented in polar form.
    /// </summary>
    [Description("Computes the cartesian coordinates of 2D vectors represented in polar form.")]
    public class PolarToCart : ArrayTransform
    {
        /// <summary>
        /// Gets or sets a value specifying whether vector angle values are measured in degrees.
        /// </summary>
        [Description("Specifies whether vector angle values are measured in degrees.")]
        public bool AngleInDegrees { get; set; }

        /// <summary>
        /// Computes the cartesian coordinates for each array of vectors in polar form
        /// in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of 2D vector fields represented by a 2-channel array or image,
        /// for which to compute the cartesian coordinates.
        /// </param>
        /// <returns>
        /// A sequence of 2-channel arrays or images, where the first channel of each
        /// element stores the x-coordinates and the second channel the y-coordinates of
        /// a 2D vector.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var channelFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var magnitude = channelFactory(input, 1);
                var angle = channelFactory(input, 1);
                var x = channelFactory(input, 1);
                var y = channelFactory(input, 1);
                var output = outputFactory(input);
                CV.Split(input, magnitude, angle, null, null);
                CV.PolarToCart(magnitude, angle, x, y, AngleInDegrees);
                CV.Merge(x, y, null, null, output);
                return output;
            });
        }

        /// <summary>
        /// Computes the cartesian coordinates for each pair of polar coordinates in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of arrays, where the first array stores the magnitude, and the
        /// second array the angle of a 2D vector field for which to compute the cartesian
        /// coordinates.
        /// </param>
        /// <returns>
        /// A sequence of pairs of arrays, where the first array stores the x-coordinates, and the
        /// second array the y-coordinates of a 2D vector.
        /// </returns>
        public IObservable<Tuple<TArray, TArray>> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
            where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var x = outputFactory(input.Item1);
                var y = outputFactory(input.Item1);
                CV.PolarToCart(input.Item1, input.Item2, x, y, AngleInDegrees);
                return Tuple.Create(x, y);
            });
        }
    }
}
