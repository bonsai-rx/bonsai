using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [Description("Finds lines in the input binary image using a probabilistic Hough transform.")]
    public class HoughLines : Transform<IplImage, LineSegment[]>
    {
        public HoughLines()
        {
            Rho = 1;
            Theta = 0.01;
            Threshold = 100;
            MaxLineGap = 10;
        }

        [Precision(2, 0.01)]
        [Range(0.1, int.MaxValue)]
        [Description("The distance resolution in units of pixels.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public double Rho { get; set; }

        [Range(0.01, Math.PI)]
        [Description("The angle resolution in radians.")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public double Theta { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The accumulator threshold. A line is returned when the corresponding accumulator is greater than this value.")]
        public int Threshold { get; set; }

        [Description("The minimum line length.")]
        public double MinLineLength { get; set; }

        [Description("The maximum gap between line segments lying on the same line in order to consider them as a single line segment.")]
        public double MaxLineGap { get; set; }

        public override IObservable<LineSegment[]> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                using (var storage = new MemStorage())
                {
                    var lines = CV.HoughLines2(
                        input,
                        storage,
                        HoughLinesMethod.Probabilistic,
                        Rho, Theta,
                        Threshold,
                        MinLineLength,
                        MaxLineGap);
                    var result = new LineSegment[lines.Count];
                    lines.CopyTo(result);
                    return result;
                }
            });
        }
    }
}
