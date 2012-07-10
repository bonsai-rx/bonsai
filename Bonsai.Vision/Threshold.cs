using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Threshold : Projection<IplImage, IplImage>
    {
        public Threshold()
        {
            ThresholdValue = 128;
            MaxValue = 255;
        }

        [Range(0, 255)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        public double MaxValue { get; set; }

        public ThresholdType ThresholdType { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, input.NumChannels);
            ImgProc.cvThreshold(input, output, ThresholdValue, MaxValue, ThresholdType);
            return output;
        }
    }
}
