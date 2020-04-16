using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [Description("Finds circles in the input grayscale image using a Hough transform.")]
    public class HoughCircles : Transform<IplImage, Circle[]>
    {
        public HoughCircles()
        {
            AccumulatorFactor = 1;
            MinDistance = 1;
            Param1 = 100;
            Param2 = 100;
        }

        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The inverse ratio of the accumulator resolution to image resolution.")]
        public double AccumulatorFactor { get; set; }

        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The minimum distance between the centers of detected circles.")]
        public double MinDistance { get; set; }

        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The higher threshold of the canny edge detector.")]
        public double Param1 { get; set; }

        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The accumulator threshold at the center detection stage.")]
        public double Param2 { get; set; }

        [Precision(0, 1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The minimum circle radius.")]
        public int MinRadius { get; set; }

        [Precision(0, 1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The maximum circle radius.")]
        public int MaxRadius { get; set; }

        public override IObservable<Circle[]> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                using (var storage = new MemStorage())
                {
                    var circles = CV.HoughCircles(
                        input, storage,
                        HoughCirclesMethod.Gradient,
                        AccumulatorFactor,
                        MinDistance,
                        Param1, Param2,
                        MinRadius, MaxRadius);
                    var output = new Circle[circles.Count];
                    circles.CopyTo(output);
                    return output;
                }
            });
        }
    }
}
