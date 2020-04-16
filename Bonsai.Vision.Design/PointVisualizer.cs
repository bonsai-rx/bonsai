using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point))]
[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point2d))]
[assembly: TypeVisualizer(typeof(PointVisualizer), Target = typeof(Point2f))]

namespace Bonsai.Vision.Design
{
    public class PointVisualizer : ObjectTextVisualizer
    {
    }
}
