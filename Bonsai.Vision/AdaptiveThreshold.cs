using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Applies an adaptive threshold to the input grayscale image.")]
    public class AdaptiveThreshold : Transform<IplImage, IplImage>
    {
        public AdaptiveThreshold()
        {
            MaxValue = 255;
            BlockSize = 3;
            Parameter = 5;
        }

        [Description("The value assigned to pixels determined to be above the threshold.")]
        public double MaxValue { get; set; }

        [Description("The adaptive threshold algorithm used to process the image.")]
        public AdaptiveThresholdMethod AdaptiveMethod { get; set; }

        [Description("The type of binary threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        [Precision(0, 2)]
        [Range(3, int.MaxValue)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the pixel neighborhood used to calculate the threshold for a pixel.")]
        public int BlockSize { get; set; }

        [Description("An algorithm dependent constant subtracted from the mean or weighted mean.")]
        public double Parameter { get; set; }

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
