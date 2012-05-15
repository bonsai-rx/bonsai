using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using OpenCV.Net;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(CvPointVisualizer), Target = typeof(CvPoint))]
[assembly: TypeVisualizer(typeof(CvPoint2D32fVisualizer), Target = typeof(CvPoint2D32f))]

namespace Bonsai.Vision.Design
{
    public class CvPointVisualizer : NumericTupleVisualizer
    {
        public override void Show(object value)
        {
            var point = (CvPoint)value;
            base.Show(Tuple.Create(point.X, point.Y));
        }
    }

    public class CvPoint2D32fVisualizer : NumericTupleVisualizer
    {
        public override void Show(object value)
        {
            var point = (CvPoint2D32f)value;
            base.Show(Tuple.Create(point.X, point.Y));
        }
    }
}
