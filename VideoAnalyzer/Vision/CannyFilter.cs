using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace VideoAnalyzer.Vision
{
    public class CannyFilter : Filter<IplImage, IplImage>
    {
        IplImage output;

        public double Threshold1 { get; set; }

        public double Threshold2 { get; set; }

        public int ApertureSize { get; set; }

        public override IplImage Process(IplImage input)
        {
            ImgProc.cvCanny(input, output, Threshold1, Threshold2, ApertureSize);
            return output;
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 1);
        }

        public override void Unload()
        {
            output.Close();
        }
    }
}
