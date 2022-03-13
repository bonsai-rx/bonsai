using System;
using OpenCV.Net;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(ScalarVisualizer), Target = typeof(Scalar))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays a sequence of <see cref="Scalar"/>
    /// values as a time series.
    /// </summary>
    public class ScalarVisualizer : TimeSeriesVisualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarVisualizer"/> class.
        /// </summary>
        public ScalarVisualizer()
            : base(numSeries: 4)
        {
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var scalar = (Scalar)value;
            AddValue(DateTime.Now, scalar.Val0, scalar.Val1, scalar.Val2, scalar.Val3);
        }
    }
}
