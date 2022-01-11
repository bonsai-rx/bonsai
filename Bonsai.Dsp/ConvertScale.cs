using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that converts each array in the sequence to the specified
    /// bit depth, with optional linear transformation.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts each array in the sequence to the specified bit depth, with optional linear transformation.")]
    public class ConvertScale : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the bit depth of each element in the output array.
        /// </summary>
        /// <remarks>
        /// If this property is not specified, the bit depth of the output array
        /// will be the same as the bit depth of the input array.
        /// </remarks>
        [TypeConverter(typeof(DepthConverter))]
        [Description("The optional bit depth of each element in the output array.")]
        public Depth? Depth { get; set; }

        /// <summary>
        /// Gets or sets the optional scale factor to apply to the array elements.
        /// </summary>
        [Description("The optional scale factor to apply to the array elements.")]
        public double Scale { get; set; } = 1;

        /// <summary>
        /// Gets or sets the optional value to be added to the scaled array elements.
        /// </summary>
        [Description("The optional value to be added to the scaled array elements.")]
        public double Shift { get; set; }

        /// <summary>
        /// Converts each array in the sequence to the specified bit depth, with
        /// optional linear transformation.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The sequence of arrays to be converted.</param>
        /// <returns>
        /// A sequence containing the converted and scaled arrays.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var outputDepthFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                var depth = Depth;
                var output = depth.HasValue
                    ? outputDepthFactory(input, depth.Value)
                    : outputFactory(input);
                CV.ConvertScale(input, output, Scale, Shift);
                return output;
            });
        }
    }
}
