using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class BackgroundSubtraction : Filter<IplImage, IplImage>
    {
        IplImage output;
        IplImage background;
        IplImage backgroundMask;
        int averageCount;

        public int BackgroundFrames { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (averageCount < BackgroundFrames)
            {
                ImgProc.cvAcc(input, background, CvArr.Null);
                averageCount++;
            }
            else if (averageCount == BackgroundFrames)
            {
                Core.cvConvertScale(background, backgroundMask, 1.0 / averageCount, 0);
                averageCount++;
            }
            else
            {
                Core.cvAbsDiff(input, backgroundMask, output);
            }

            return output;
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            output = new IplImage(size, 8, 1);
            background = new IplImage(size, 32, 1);
            backgroundMask = new IplImage(size, 8, 1);
            background.SetZero();
            backgroundMask.SetZero();
            output.SetZero();
            averageCount = 0;
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            output.Close();
            base.Unload(context);
        }
    }
}
