using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Applies a fixed threshold to the input image.")]
    public class Threshold : Transform<IplImage, IplImage>
    {
        public Threshold()
        {
            ThresholdValue = 128;
            MaxValue = 255;
        }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test individual pixels.")]
        public double ThresholdValue { get; set; }

        [Description("The value assigned to pixels determined to be above the threshold.")]
        public double MaxValue { get; set; }

        [Description("The type of threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Threshold(input, output, ThresholdValue, MaxValue, ThresholdType);
                return output;
            });
        }
    }
}
