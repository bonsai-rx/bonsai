using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes the running average of all the arrays in the sequence.
    /// </summary>
    [Description("Computes the running average of all the arrays in the sequence.")]
    public class RunningAverage : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the weight to assign to each new array in the sequence.
        /// This parameter determines how fast the average forgets previous values.
        /// </summary>
        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The weight to assign to each new array in the sequence. This parameter determines how fast the average forgets previous values.")]
        public double Alpha { get; set; }

        /// <summary>
        /// Computes the running average of all the arrays in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel arrays, where each element represents the weighted
        /// sum of the corresponding input value and the accumulated average.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var accumulatorFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return Observable.Defer(() =>
            {
                TArray accumulator = null;
                return source.Select(input =>
                {
                    if (accumulator == null)
                    {
                        accumulator = accumulatorFactory(input, Depth.F32);
                        CV.Convert(input, accumulator);
                        return input;
                    }
                    else
                    {
                        var output = outputFactory(input);
                        CV.RunningAvg(input, accumulator, Alpha);
                        CV.Convert(accumulator, output);
                        return output;
                    }
                });
            });
        }
    }
}
