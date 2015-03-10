using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point))]
[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point2d))]
[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point2f))]

namespace Bonsai.Vision.Design
{
    public class PointVisualizer : ObjectTextVisualizer
    {
    }
}
