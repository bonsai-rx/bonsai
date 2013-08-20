using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    [Description("Applies an adaptive threshold to the input image.")]
    public class AdaptiveThreshold : Selector<IplImage, IplImage>
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
        public ThresholdType ThresholdType { get; set; }

        [Precision(0, 2)]
        [Range(3, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The size of the pixel neighborhood used to calculate the threshold for a pixel.")]
        public int BlockSize { get; set; }

        [Description("An algorithm dependent constant subtracted from the mean or weighted mean.")]
        public double Parameter { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            ImgProc.cvAdaptiveThreshold(input, output, MaxValue, AdaptiveMethod, ThresholdType, BlockSize, Parameter);
            return output;
        }
    }
}
