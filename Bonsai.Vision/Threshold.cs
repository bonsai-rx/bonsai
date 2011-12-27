using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Threshold : Filter<IplImage, IplImage>
    {
        IplImage output;

        public double ThresholdValue { get; set; }

        public double MaxValue { get; set; }

        public ThresholdType ThresholdType { get; set; }

        public override IplImage Process(IplImage input)
        {
            ImgProc.cvThreshold(input, output, ThresholdValue, MaxValue, ThresholdType);
            return output;
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 1);
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            output.Close();
            base.Unload(context);
        }
    }
}
