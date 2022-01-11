using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that applies a fixed threshold to each element in the sequence.
    /// </summary>
    [Description("Applies a fixed threshold to each element in the sequence.")]
    public class Threshold : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the threshold value used to test individual samples.
        /// </summary>
        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test individual samples.")]
        public double ThresholdValue { get; set; }

        /// <summary>
        /// Gets or sets the value assigned to samples determined to be above the threshold.
        /// </summary>
        [Description("The value assigned to samples determined to be above the threshold.")]
        public double MaxValue { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the type of threshold to apply to
        /// individual samples.
        /// </summary>
        [Description("Specifies the type of threshold to apply to individual samples.")]
        public ThresholdTypes ThresholdType { get; set; }

        /// <summary>
        /// Applies a fixed threshold to each element in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D matrix values.
        /// </param>
        /// <returns>
        /// A sequence of 2D matrix values, where the values in each matrix are
        /// set by applying the threshold operation specified in <see cref="ThresholdType"/>.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var output = new Mat(input.Rows, input.Cols, input.Depth, input.Channels);
                CV.Threshold(input, output, ThresholdValue, MaxValue, ThresholdType);
                return output;
            });
        }
    }
}
