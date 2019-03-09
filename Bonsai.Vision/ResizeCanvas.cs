using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Resizes the input image canvas without stretching the image.")]
    public class ResizeCanvas : Transform<IplImage, IplImage>
    {
        public ResizeCanvas()
        {
            BorderType = IplBorder.Constant;
        }

        [Description("The size of the output image.")]
        public Size Size { get; set; }

        [Description("The method used to generate values on the canvas border.")]
        public IplBorder BorderType { get; set; }

        [Description("The value to which constant border pixels will be set to.")]
        public Scalar FillValue { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional top-left coordinates where the source image will be placed.")]
        public Point? Offset { get; set; }

        static void AdjustRectangle(ref int left, int right, ref int origin, ref int extent)
        {
            if (left < 0)
            {
                origin -= left;
                extent += left;
                left = 0;
            }
            if (right < 0)
            {
                extent += right;
            }
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var targetSize = Size;
                if (targetSize.Width == 0) targetSize.Width = input.Width;
                if (targetSize.Height == 0) targetSize.Height = input.Height;

                Point offset;
                var offsetNullable = Offset;
                if (offsetNullable.HasValue) offset = offsetNullable.Value;
                else
                {
                    offset.X = (targetSize.Width - input.Width) / 2;
                    offset.Y = (targetSize.Height - input.Height) / 2;
                }

                var right = targetSize.Width - offset.X - input.Width;
                var bottom = targetSize.Height - offset.Y - input.Height;
                if (offset.X == 0 && offset.Y == 0 && right == 0 && bottom == 0) return input;

                var inputRect = new Rect(0, 0, input.Width, input.Height);
                AdjustRectangle(ref offset.X, right, ref inputRect.X, ref inputRect.Width);
                AdjustRectangle(ref offset.Y, bottom, ref inputRect.Y, ref inputRect.Height);
                if (offset.X <= 0 && offset.Y <= 0 && right <= 0 && bottom <= 0)
                {
                    return input.GetSubRect(inputRect);
                }

                var output = new IplImage(targetSize, input.Depth, input.Channels);
                using (var inputHeader = input.GetSubRect(inputRect))
                {
                    CV.CopyMakeBorder(inputHeader, output, offset, BorderType, FillValue);
                }
                return output;
            });
        }
    }
}
