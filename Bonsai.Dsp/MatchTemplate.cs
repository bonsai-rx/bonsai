using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that compares a template against overlapping regions of each array in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Compares a template against overlapping regions of each array in the sequence.")]
    public class MatchTemplate
    {
        /// <summary>
        /// Gets or sets a value specifying the method used to compare the template
        /// with overlapping array regions.
        /// </summary>
        [Description("Specifies the method used to compare the template with overlapping array regions.")]
        public TemplateMatchingMethod MatchingMethod { get; set; } = TemplateMatchingMethod.CorrelationCoefficientNormed;

        /// <summary>
        /// Compares a template against overlapping regions of each array in
        /// an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects to compare with the template.
        /// </typeparam>
        /// <typeparam name="TTemplate">
        /// The type of the array-like objects representing the template to compare
        /// against arrays in the sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of multi-channel arrays representing respectively the
        /// values and the template used to compute the map of comparison results.
        /// </param>
        /// <returns>
        /// A single-channel array of 32-bit floating-point values representing a map of
        /// the comparison results for each overlapping region.
        /// </returns>
        public IObservable<TArray> Process<TArray, TTemplate>(IObservable<Tuple<TArray, TTemplate>> source)
            where TArray : Arr
            where TTemplate : Arr
        {
            var outputFactory = ArrFactory<TArray>.DefaultFactory;
            return source.Select(input =>
            {
                var image = input.Item1;
                var template = input.Item2;
                var outputSize = image.Size;
                var templateSize = template.Size;
                outputSize.Width -= templateSize.Width - 1;
                outputSize.Height -= templateSize.Height - 1;
                var output = outputFactory(outputSize, Depth.F32, 1);
                CV.MatchTemplate(image, template, output, MatchingMethod);
                return output;
            });
        }
    }
}
