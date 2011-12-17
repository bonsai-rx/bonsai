using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class GrayscaleFilter : Filter<IplImage, IplImage>
    {
        IplImage output;

        public override IplImage Process(IplImage input)
        {
            ImgProc.cvCvtColor(input, output, ColorConversion.BGR2GRAY);
            return output;
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 1);
        }

        public override void Unload(WorkflowContext context)
        {
            output.Close();
        }
    }
}
