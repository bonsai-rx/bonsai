using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
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
