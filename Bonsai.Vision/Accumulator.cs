using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Accumulator : Transform<IplImage, IplImage>
    {
        IplImage sum;

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            sum = IplImageHelper.EnsureImageFormat(sum, input.Size, 32, input.NumChannels);
            ImgProc.cvAcc(input, sum, CvArr.Null);
            Core.cvConvert(sum, output);
            return output;
        }

        protected override void Unload()
        {
            if (sum != null)
            {
                sum.Dispose();
                sum = null;
            }
            base.Unload();
        }
    }
}
