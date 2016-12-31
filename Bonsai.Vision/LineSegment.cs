using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LineSegment
    {
        public Point Start;
        public Point End;

        public override string ToString()
        {
            return string.Format("Line2f(Start={0}, End={1})", Start, End);
        }
    }
}
