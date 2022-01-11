using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the magnitude of 2D vector elements in the sequence.
    /// </summary>
    [Description("Calculates the magnitude of 2D vector elements in the sequence.")]
    public class Magnitude : ArrayTransform
    {
        /// <summary>
        /// Calculates the magnitude of pairs of one-dimensional arrays in an observable sequence,
        /// where each pair represents a 2D vector element.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// A sequence of pairs of one-dimensional arrays, where each array represents
        /// respectively the X and Y coordinates of a 2D vector.
        /// <returns>
        /// A sequence of single-channel arrays where each element represents the magnitude
        /// of the corresponding 2D vector.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input.Item1);
                CV.CartToPolar(input.Item1, input.Item2, output);
                return output;
            });
        }

        /// <summary>
        /// Calculates the magnitude of 2D vector elements in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of two-channel arrays where each element represents a 2D vector.
        /// </param>
        /// <returns>
        /// A sequence of single-channel arrays where each element represents the magnitude
        /// of the corresponding 2D vector.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            return Observable.Defer(() =>
            {
                TArray x = null;
                TArray y = null;
                return source.Select(input =>
                {
                    if (x == null)
                    {
                        x = outputFactory(input, 1);
                        y = outputFactory(input, 1);
                    }

                    var output = outputFactory(input, 1);
                    CV.Split(input, x, y, null, null);
                    CV.CartToPolar(x, y, output);
                    return output;
                });
            });
        }
    }
}
