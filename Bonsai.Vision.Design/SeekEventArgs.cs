using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class SeekEventArgs : EventArgs
    {
        public SeekEventArgs(int frameNumber)
        {
            FrameNumber = frameNumber;
        }

        public int FrameNumber { get; private set; }
    }
}
