using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class BackgroundSubtraction : Projection<IplImage, IplImage>
    {
        IplImage background;
        IplImage backgroundMask;
        int averageCount;

        public int BackgroundFrames { get; set; }

        public override IplImage Process(IplImage input)
        {
            background = IplImageHelper.EnsureImageFormat(background, input.Size, 32, 1);
            backgroundMask = IplImageHelper.EnsureImageFormat(backgroundMask, input.Size, 8, 1);
            if (averageCount == 0)
            {
                background.SetZero();
                backgroundMask.SetZero();
            }

            var output = new IplImage(input.Size, 8, 1);
            if (averageCount < BackgroundFrames)
            {
                output.SetZero();
                ImgProc.cvAcc(input, background, CvArr.Null);
                averageCount++;
            }
            else if (averageCount == BackgroundFrames)
            {
                output.SetZero();
                Core.cvConvertScale(background, backgroundMask, 1.0 / averageCount, 0);
                averageCount++;
            }
            else
            {
                Core.cvAbsDiff(input, backgroundMask, output);
            }

            return output;
        }

        protected override void Unload()
        {
            averageCount = 0;
            if (background != null)
            {
                background.Close();
                backgroundMask.Close();
                background = backgroundMask = null;
            }
            base.Unload();
        }
    }
}
