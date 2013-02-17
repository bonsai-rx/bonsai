using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Dsp
{
    [Description("Applies a fixed threshold to the input signal.")]
    public class Threshold : Transform<CvMat, CvMat>
    {
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The threshold value used to test individual samples.")]
        public double ThresholdValue { get; set; }

        [Description("The value assigned to samples determined to be above the threshold.")]
        public double MaxValue { get; set; }

        [Description("The type of threshold to apply to individual samples.")]
        public ThresholdType ThresholdType { get; set; }

        public override CvMat Process(CvMat input)
        {
            var output = new CvMat(input.Rows, input.Cols, input.Depth, input.NumChannels);
            ImgProc.cvThreshold(input, output, ThresholdValue, MaxValue, ThresholdType);
            return output;
        }
    }
}
