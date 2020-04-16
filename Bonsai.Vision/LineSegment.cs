using OpenCV.Net;
using System.Runtime.InteropServices;

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
