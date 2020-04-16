using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Circle
    {
        public Point2f Center;
        public float Radius;

        public override string ToString()
        {
            return string.Format("Circle(Center={0}, Radius={1})", Center, Radius);
        }
    }
}
