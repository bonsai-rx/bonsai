using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Shifts the elements of the input array by a specified offset.")]
    public class Shift : Transform<Mat, Mat>
    {
        public Shift()
        {
            BorderType = IplBorder.Wrap;
        }

        [Description("The offset by which to shift the input buffer in either direction.")]
        public Point Offset { get; set; }

        [Description("The method used to generate values on the border of the shift.")]
        public IplBorder BorderType { get; set; }

        [Description("The value to which constant border pixels will be set to.")]
        public Scalar FillValue { get; set; }

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
