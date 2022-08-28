using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds circles on each grayscale image in the
    /// sequence using a Hough transform.
    /// </summary>
    [Description("Finds circles on each grayscale image in the sequence using a Hough transform.")]
    public class HoughCircles : Transform<IplImage, Circle[]>
    {
        /// <summary>
        /// Gets or sets the inverse ratio of the accumulator resolution to image resolution.
        /// </summary>
        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The inverse ratio of the accumulator resolution to image resolution.")]
        public double AccumulatorFactor { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minimum distance between the centers of detected circles.
        /// </summary>
        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The minimum distance between the centers of detected circles.")]
        public double MinDistance { get; set; } = 1;

        /// <summary>
        /// Gets or sets the higher threshold of the canny edge detector.
        /// </summary>
        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The higher threshold of the canny edge detector.")]
        public double Param1 { get; set; } = 100;

        /// <summary>
        /// Gets or sets the accumulator threshold at the center detection stage.
        /// </summary>
        [Precision(0, 1)]
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The accumulator threshold at the center detection stage.")]
        public double Param2 { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value specifying the minimum circle radius.
        /// </summary>
        [Precision(0, 1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the minimum circle radius.")]
        public int MinRadius { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the maximum circle radius.
        /// </summary>
        [Precision(0, 1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the maximum circle radius.")]
        public int MaxRadius { get; set; }

        /// <summary>
        /// Finds circles on each grayscale image in an observable sequence using
        /// a Hough transform.
        /// </summary>
        /// <param name="source">
        /// The sequence of images on which to find Hough circles.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Circle"/> arrays representing the circles
        /// extracted from each image in the <paramref name="source"/> sequence.
        /// </returns>
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
