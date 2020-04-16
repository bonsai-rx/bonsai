using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Applies a fixed threshold to the input signal.")]
    public class Threshold : Transform<Mat, Mat>
    {
        public Threshold()
        {
            MaxValue = 1;
        }

        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test individual samples.")]
        public double ThresholdValue { get; set; }

        [Description("The value assigned to samples determined to be above the threshold.")]
        public double MaxValue { get; set; }

        [Description("The type of threshold to apply to individual samples.")]
        public ThresholdTypes ThresholdType { get; set; }

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
