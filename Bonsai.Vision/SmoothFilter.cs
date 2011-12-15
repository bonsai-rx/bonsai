using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class SmoothFilter : Filter<IplImage, IplImage>
    {
        IplImage output;

        public SmoothMethod SmoothType { get; set; }

        public int Size1 { get; set; }

        public int Size2 { get; set; }

        public double Sigma1 { get; set; }

        public double Sigma2 { get; set; }

        public override IplImage Process(IplImage input)
        {
            ImgProc.cvSmooth(input, output, SmoothType, Size1, Size2, Sigma1, Sigma2);
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
