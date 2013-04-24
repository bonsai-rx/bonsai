using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class SelectChannels : Transform<CvMat, CvMat>
    {
        public SelectChannels()
        {
            Length = 1;
        }

        public int StartIndex { get; set; }

        public int Length { get; set; }

        public override CvMat Process(CvMat input)
        {
            return input.GetSubRect(new CvRect(0, StartIndex, input.Cols, Length));
        }
    }
}
