using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class ColorBalance : Transform<IplImage, IplImage>
    {
        IplImage channel1;
        IplImage channel2;
        IplImage channel3;
        IplImage channel4;

        public ColorBalance()
        {
            Scale = CvScalar.All(1);
        }

        [Precision(2, .01)]
        [Range(0, int.MaxValue)]
        [TypeConverter("Bonsai.Vision.Design.BgraScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Scale { get; set; }

        public override IplImage Process(IplImage input)
        {
            channel1 = IplImageHelper.EnsureImageFormat(channel1, input.Size, 8, 1);
            if (input.NumChannels > 1) channel2 = IplImageHelper.EnsureImageFormat(channel2, input.Size, 8, 1);
            if (input.NumChannels > 2) channel3 = IplImageHelper.EnsureImageFormat(channel3, input.Size, 8, 1);
            if (input.NumChannels > 3) channel4 = IplImageHelper.EnsureImageFormat(channel4, input.Size, 8, 1);

            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvSplit(input, channel1, channel2 ?? CvArr.Null, channel3 ?? CvArr.Null, channel4 ?? CvArr.Null);

            if (channel1 != null) Core.cvConvertScale(channel1, channel1, Scale.Val0, 0);
            if (channel2 != null) Core.cvConvertScale(channel2, channel2, Scale.Val1, 0);
            if (channel3 != null) Core.cvConvertScale(channel3, channel3, Scale.Val2, 0);
            if (channel4 != null) Core.cvConvertScale(channel4, channel4, Scale.Val3, 0);
            Core.cvMerge(channel1, channel2 ?? CvArr.Null, channel3 ?? CvArr.Null, channel4 ?? CvArr.Null, output);
            return output;
        }

        protected override void Unload()
        {
            if (channel1 != null)
            {
                channel1.Close();
                channel1 = null;
            }

            if (channel2 != null)
            {
                channel2.Close();
                channel2 = null;
            }

            if (channel3 != null)
            {
                channel3.Close();
                channel3 = null;
            }

            if (channel4 != null)
            {
                channel4.Close();
                channel4 = null;
            }

            base.Unload();
        }
    }
}
