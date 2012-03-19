using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class RunningAverage : Projection<IplImage, IplImage>
    {
        IplImage accumulator;

        public double Alpha { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (accumulator == null)
            {
                accumulator = new IplImage(input.Size, 32, input.NumChannels);
            }

            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            ImgProc.cvRunningAvg(input, accumulator, Alpha, CvArr.Null);
            Core.cvConvertScale(accumulator, output, 1, 0);
            return output;
        }

        protected override void Unload()
        {
            if (accumulator != null)
            {
                accumulator.Close();
                accumulator = null;
            }
            base.Unload();
        }
    }
}
