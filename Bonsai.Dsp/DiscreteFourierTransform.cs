using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that performs a forward or inverse discrete Fourier transform
    /// on each 1D or 2D array in the sequence.
    /// </summary>
    [Description("Performs a forward or inverse discrete Fourier transform on each 1D or 2D array in the sequence.")]
    public class DiscreteFourierTransform : ArrayTransform
    {
        /// <summary>
        /// Gets or sets a value specifying the operation of the discrete Fourier transform.
        /// </summary>
        [Description("Specifies the operation of the Discrete Fourier transform.")]
        public DiscreteTransformFlags OperationFlags { get; set; }

        /// <summary>
        /// Performs a forward or inverse discrete Fourier transform on each 1D or 2D array
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of array-like objects for which to compute the discrete
        /// Fourier transform.
        /// </param>
        /// <returns>
        /// A sequence of two-channel array of complex numbers representing the discrete
        /// Fourier transform of each array in the input sequence.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeFactory;
            var inputFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input, Depth.F32, 2);
                if (input.ElementType != output.ElementType)
                {
                    var temp = inputFactory(input, Depth.F32);
                    CV.Convert(input, temp);
                    input = temp;
                }

                CV.DFT(input, output, OperationFlags, 0);
                return output;
            });
        }
    }
}
