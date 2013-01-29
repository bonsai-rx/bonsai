using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class SelectChannel : Transform<IplImage, IplImage>
    {
        [Range(0, 3)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int Channel { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, 1);
            var channel = Channel;
            if (channel < 0 || channel >= input.NumChannels) output.SetZero();
            else
            {
                switch (channel)
                {
                    case 0: Core.cvSplit(input, output, CvArr.Null, CvArr.Null, CvArr.Null); break;
                    case 1: Core.cvSplit(input, CvArr.Null, output, CvArr.Null, CvArr.Null); break;
                    case 2: Core.cvSplit(input, CvArr.Null, CvArr.Null, output, CvArr.Null); break;
                    case 3: Core.cvSplit(input, CvArr.Null, CvArr.Null, CvArr.Null, output); break;
                    default: throw new InvalidOperationException("Invalid channel number.");
                }
            }

            return output;
        }
    }
}
