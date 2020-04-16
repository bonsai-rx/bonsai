using System;
using OpenCV.Net;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(ScalarVisualizer), Target = typeof(Scalar))]

namespace Bonsai.Vision.Design
{
    public class ScalarVisualizer : TimeSeriesVisualizer
    {
        public ScalarVisualizer()
            : base(4)
        {
        }

        public override void Show(object value)
        {
            var scalar = (Scalar)value;
            AddValue(DateTime.Now, scalar.Val0, scalar.Val1, scalar.Val2, scalar.Val3);
        }
    }
}
