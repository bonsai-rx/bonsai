using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds lines on each binary image in the sequence
    /// using a probabilistic Hough transform.
    /// </summary>
    [Description("Finds lines on each binary image in the sequence using a probabilistic Hough transform.")]
    public class HoughLines : Transform<IplImage, LineSegment[]>
    {
        /// <summary>
        /// Gets or sets the distance resolution, in units of pixels.
        /// </summary>
        [Precision(2, 0.01)]
        [Range(0.1, int.MaxValue)]
        [Description("The distance resolution, in units of pixels.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public double Rho { get; set; } = 1;

        /// <summary>
        /// Gets or sets the angle resolution, in radians.
        /// </summary>
        [Range(0.01, Math.PI)]
        [Description("The angle resolution, in radians.")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public double Theta { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the accumulator threshold. A line is returned when the
        /// corresponding accumulator is greater than this value.
        /// </summary>
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The accumulator threshold. A line is returned when the corresponding accumulator is greater than this value.")]
        public int Threshold { get; set; } = 100;

        /// <summary>
        /// Gets or sets the minimum line length.
        /// </summary>
        [Description("The minimum line length.")]
        public double MinLineLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum gap between line segments lying on the same
        /// line in order to consider them as a single line segment.
        /// </summary>
        [Description("The maximum gap between line segments lying on the same line in order to consider them as a single line segment.")]
        public double MaxLineGap { get; set; } = 10;

        /// <summary>
        /// Finds lines on each binary image in an observable sequence using a
        /// probabilistic Hough transform.
        /// </summary>
        /// <param name="source">
        /// The sequence of images on which to find Hough lines.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="LineSegment"/> arrays representing the lines
        /// extracted from each image in the <paramref name="source"/> sequence.
        /// </returns>
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
