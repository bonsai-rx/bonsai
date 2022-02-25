using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies an adaptive threshold to every grayscale
    /// image in the sequence.
    /// </summary>
    [Description("Applies an adaptive threshold to every grayscale image in the sequence.")]
    public class AdaptiveThreshold : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the value assigned to pixels determined to be above the threshold.
        /// </summary>
        [Description("The value assigned to pixels determined to be above the threshold.")]
        public double MaxValue { get; set; } = 255;

        /// <summary>
        /// Gets or sets a value specifying the adaptive threshold algorithm used
        /// to process the image.
        /// </summary>
        [Description("Specifies the adaptive threshold algorithm used to process the image.")]
        public AdaptiveThresholdMethod AdaptiveMethod { get; set; }

        /// <summary>
        /// Gets or sets the a value specifying the type of binary threshold to apply
        /// to individual pixels.
        /// </summary>
        [Description("Specifies the type of binary threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        /// <summary>
        /// Gets or sets the size of the pixel neighborhood used to calculate the
        /// threshold for a pixel.
        /// </summary>
        [Precision(0, 2)]
        [Range(3, int.MaxValue)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the pixel neighborhood used to calculate the threshold for a pixel.")]
        public int BlockSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets an algorithm dependent constant subtracted from the mean or weighted mean.
        /// </summary>
        [Description("An algorithm dependent constant subtracted from the mean or weighted mean.")]
        public double Parameter { get; set; } = 5;

        /// <summary>
        /// Applies an adaptive threshold to every grayscale image in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">The sequence of images to threshold.</param>
        /// <returns>The sequence of thresholded images.</returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                CV.AdaptiveThreshold(input, output, MaxValue, AdaptiveMethod, ThresholdType, BlockSize, Parameter);
                return output;
            });
        }
    }
}
