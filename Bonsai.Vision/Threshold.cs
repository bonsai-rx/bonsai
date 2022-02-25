using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a fixed threshold to each image
    /// in the sequence.
    /// </summary>
    [Description("Applies a fixed threshold to each image in the sequence.")]
    public class Threshold : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the threshold value used to test individual pixels.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test individual pixels.")]
        public double ThresholdValue { get; set; } = 128;

        /// <summary>
        /// Gets or sets the value assigned to pixels determined to be above the threshold.
        /// </summary>
        [Description("The value assigned to pixels determined to be above the threshold.")]
        public double MaxValue { get; set; } = 255;

        /// <summary>
        /// Gets or sets the type of threshold to apply to individual pixels.
        /// </summary>
        [Description("The type of threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        /// <summary>
        /// Applies a fixed threshold to each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to threshold.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each non-zero pixel represents
        /// a value in the original image that was accepted by the threshold criteria.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (input.Depth == IplDepth.U16)
                {
                    var temp = new IplImage(input.Size, IplDepth.F32, input.Channels);
                    CV.Convert(input, temp);
                    input = temp;
                }

                var output = new IplImage(input.Size, IplDepth.U8, input.Channels);
                CV.Threshold(input, output, ThresholdValue, MaxValue, ThresholdType);
                return output;
            });
        }
    }
}
