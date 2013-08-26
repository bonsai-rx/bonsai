using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using OpenCV.Net;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(CvScalarVisualizer), Target = typeof(CvScalar))]

namespace Bonsai.Vision.Design
{
    public class CvScalarVisualizer : TimeSeriesVisualizer
    {
        public CvScalarVisualizer()
            : base(4)
        {
        }

        public override void Show(object value)
        {
            var scalar = (CvScalar)value;
            AddValue(DateTime.Now, scalar.Val0, scalar.Val1, scalar.Val2, scalar.Val3);
        }
    }
}
