using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class IncrementalMean : Projection<IplImage, IplImage>
    {
        int count;
        IplImage mean;

        public override IplImage Process(IplImage input)
        {
            if (mean == null)
            {
                mean = new IplImage(input.Size, input.Depth, input.NumChannels);
                mean.SetZero();
            }

            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvSub(input, mean, output, CvArr.Null);
            Core.cvConvertScale(output, output, 1f / count++, 0);
            Core.cvAdd(mean, output, mean, CvArr.Null);
            Core.cvCopy(mean, output, CvArr.Null);
            return output;
        }

        public override IDisposable Load()
        {
            count = 0;
            return base.Load();
        }

        protected override void Unload()
        {
            if (mean != null)
            {
                mean.Close();
                mean = null;
            }
            base.Unload();
        }
    }
}
