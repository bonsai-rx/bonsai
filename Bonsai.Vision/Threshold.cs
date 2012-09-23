using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    [Description("Applies a fixed threshold to the input image.")]
    public class Threshold : Projection<IplImage, IplImage>
    {
        public Threshold()
        {
            ThresholdValue = 128;
            MaxValue = 255;
        }

        [Range(0, 255)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        [Description("The threshold value used to test individual pixels.")]
        public double ThresholdValue { get; set; }

        [Description("The value assigned to pixels determined to be above the threshold.")]
        public double MaxValue { get; set; }

        [Description("The type of threshold to apply to individual pixels.")]
        public ThresholdType ThresholdType { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, input.NumChannels);
            ImgProc.cvThreshold(input, output, ThresholdValue, MaxValue, ThresholdType);
            return output;
        }
    }
}
