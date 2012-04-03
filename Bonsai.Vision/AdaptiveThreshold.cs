using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class AdaptiveThreshold : Projection<IplImage, IplImage>
    {
        public AdaptiveThreshold()
        {
            MaxValue = 255;
            BlockSize = 3;
            Parameter = 5;
        }

        public double MaxValue { get; set; }

        public AdaptiveThresholdMethod AdaptiveMethod { get; set; }

        public ThresholdType ThresholdType { get; set; }

        [Precision(0, 2)]
        [Range(3, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int BlockSize { get; set; }

        public double Parameter { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            ImgProc.cvAdaptiveThreshold(input, output, MaxValue, AdaptiveMethod, ThresholdType, BlockSize, Parameter);
            return output;
        }
    }
}
