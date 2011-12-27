using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai;

namespace VideoAnalyzer.Vision
{
    public class Not : Filter<IplImage, IplImage>
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
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            output.Close();
            base.Unload(context);
        }
    }
}
