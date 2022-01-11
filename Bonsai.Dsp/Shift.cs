using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that shifts the elements of each array in the sequence
    /// by a specified offset.
    /// </summary>
    [Description("Shifts the elements of each array in the sequence by a specified offset.")]
    public class Shift : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the offset by which to shift the input buffer in the
        /// horizontal and vertical direction.
        /// </summary>
        [Description("The offset by which to shift the input buffer in the horizontal and vertical direction.")]
        public Point Offset { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the method used to generate values on the
        /// border of the shift.
        /// </summary>
        [Description("Specifies the method used to generate values on the border of the shift.")]
        public IplBorder BorderType { get; set; } = IplBorder.Wrap;

        /// <summary>
        /// Gets or sets the value to which constant border pixels will be set to.
        /// </summary>
        [Description("The value to which constant border pixels will be set to.")]
        public Scalar FillValue { get; set; }

        /// <summary>
        /// Shifts the elements of each matrix in an observable sequence by a specified
        /// offset.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D matrix values.
        /// </param>
        /// <returns>
        /// A sequence of 2D matrix values, where the elements in each matrix are
        /// shifted by the specified offset in the horizontal and vertical direction.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var offset = Offset;
                var rows = input.Rows + Math.Abs(offset.Y);
                var cols = input.Cols + Math.Abs(offset.X);
                var copyOffset = new Point(Math.Max(0, offset.X), Math.Max(0, offset.Y));
                var output = new Mat(rows, cols, input.Depth, input.Channels);
                CV.CopyMakeBorder(input, output, copyOffset, BorderType, FillValue);
                output = output.GetSubRect(new Rect(
                    Math.Max(0, -offset.X),
                    Math.Max(0, -offset.Y),
                    input.Cols,
                    input.Rows));
                return output;
            });
        }

        /// <summary>
        /// Shifts the elements of each image in an observable sequence by a specified
        /// offset.
        /// </summary>
        /// <param name="source">
        /// A sequence of image values.
        /// </param>
        /// <returns>
        /// A sequence of image values, where the elements in each image are
        /// shifted by the specified offset in the horizontal and vertical direction.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var offset = Offset;
                var copyOffset = new Point(Math.Max(0, offset.X), Math.Max(0, offset.Y));
                var size = new Size(input.Width + Math.Abs(offset.X), input.Height + Math.Abs(offset.Y));
                Arr output = new IplImage(size, input.Depth, input.Channels);
                CV.CopyMakeBorder(input, output, copyOffset, BorderType, FillValue);
                output = output.GetSubRect(new Rect(
                    Math.Max(0, -offset.X),
                    Math.Max(0, -offset.Y),
                    input.Width,
                    input.Height));
                return output.GetImage();
            });
        }
    }
}
