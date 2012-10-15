using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class MotionSegmentation : Transform<IplImage, IplImage>
    {
        IplImage temp;
        IplImage accumulator;

        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double Alpha { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (accumulator == null)
            {
                accumulator = new IplImage(input.Size, 32, input.NumChannels);
                temp = new IplImage(accumulator.Size, accumulator.Depth, accumulator.NumChannels);
            }

            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvSub(input, accumulator, temp, CvArr.Null);
            ImgProc.cvRunningAvg(input, accumulator, Alpha, CvArr.Null);
            Core.cvConvertScale(temp, output, 1, 0);
            return output;
        }

        protected override void Unload()
        {
            if (accumulator != null)
            {
                accumulator.Close();
                temp.Close();
                accumulator = null;
                temp = null;
            }
            base.Unload();
        }
    }
}
