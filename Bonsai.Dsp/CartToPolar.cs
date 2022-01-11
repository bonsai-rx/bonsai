using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes the magnitude and angle of each array
    /// of 2D vectors in the sequence.
    /// </summary>
    [Description("Computes the magnitude and angle of each array of 2D vectors in the sequence.")]
    public class CartToPolar : ArrayTransform
    {
        /// <summary>
        /// Gets or sets a value specifying whether vector angle values are measured in degrees.
        /// </summary>
        [Description("Specifies whether vector angle values are measured in degrees.")]
        public bool AngleInDegrees { get; set; }

        /// <summary>
        /// Computes the magnitude and angle of each array of 2D vectors in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of 2D vector fields represented by a 2-channel array or image,
        /// for which to compute the magnitude and angle.
        /// </param>
        /// <returns>
        /// A sequence of 2-channel arrays or images, where the first channel of each
        /// element stores the magnitude and the second channel the angle of a 2D vector.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var channelFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var x = channelFactory(input, 1);
                var y = channelFactory(input, 1);
                var magnitude = channelFactory(input, 1);
                var angle = channelFactory(input, 1);
                var output = outputFactory(input);
                CV.Split(input, x, y, null, null);
                CV.CartToPolar(x, y, magnitude, angle, AngleInDegrees);
                CV.Merge(magnitude, angle, null, null, output);
                return output;
            });
        }

        /// <summary>
        /// Computes the magnitude and angle for each pair of cartesian coordinates in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of arrays, where the first array stores the x-coordinates, and the
        /// second array the y-coordinates of a 2D vector field for which to compute the magnitude
        /// and angle.
        /// </param>
        /// <returns>
        /// A sequence of pairs of arrays, where the first array stores the magnitude, and the second
        /// array stores the angle of a 2D vector.
        /// </returns>
        public IObservable<Tuple<TArray, TArray>> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
            where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var magnitude = outputFactory(input.Item1);
                var angle = outputFactory(input.Item1);
                CV.CartToPolar(input.Item1, input.Item2, magnitude, angle, AngleInDegrees);
                return Tuple.Create(magnitude, angle);
            });
        }
    }
}
