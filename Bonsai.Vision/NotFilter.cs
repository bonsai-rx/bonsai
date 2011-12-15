using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai;

namespace VideoAnalyzer.Vision
{
    public class NotFilter : Filter<IplImage, IplImage>
    {
        IplImage output;

        public override IplImage Process(IplImage input)
        {
            Core.cvNot(input, output);
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
