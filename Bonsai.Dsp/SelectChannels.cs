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
            Step = 1;
        }

        public int Start { get; set; }

        public int Stop { get; set; }

        public int Step { get; set; }

        public override CvMat Process(CvMat input)
        {
            return input.GetRows(Start, Stop, Step);
        }
    }
}
