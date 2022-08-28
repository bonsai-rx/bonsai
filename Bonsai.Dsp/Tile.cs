using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that replicates each array in the sequence along the
    /// horizontal or vertical dimensions.
    /// </summary>
    [Description("Replicates each array in the sequence along the horizontal or vertical dimensions.")]
    public class Tile : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the number of times to repeat each array in the vertical dimension.
        /// </summary>
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of times to repeat each array in the vertical dimension.")]
        public int RowTiles { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of times to repeat each array in the horizontal dimension.
        /// </summary>
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of times to repeat each array in the horizontal dimension.")]
        public int ColumnTiles { get; set; } = 1;

        /// <summary>
        /// Replicates each array in an observable sequence along the horizontal or
        /// vertical dimensions.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each array is created by
        /// replicating the original array along the horizontal or vertical dimension.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                var size = input.Size;
                var output = outputFactory(input, new Size(ColumnTiles * size.Width, RowTiles * size.Height));
                CV.Repeat(input, output);
                return output;
            });
        }
    }
}
