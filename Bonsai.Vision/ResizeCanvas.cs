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

        [Description("The interpolation method used to transform individual image elements.")]
        public IplBorder BorderType { get; set; }

        [Description("The value to which all border pixels will be set to.")]
        public Scalar FillValue { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var targetSize = Size;
                var top = targetSize.Height - input.Height;
                var left = targetSize.Width - input.Width;
                if (top == 0 && left == 0) return input;
                else
                {
                    var inputRect = new Rect(0, 0, input.Width, input.Height);
                    if (left < 0)
                    {
                        left = 0;
                        inputRect.X = input.Width / 2 - targetSize.Width / 2;
                        inputRect.Width = targetSize.Width;
                    }
                    if (top < 0)
                    {
                        top = 0;
                        inputRect.Y = input.Height / 2 - targetSize.Height / 2;
                        inputRect.Height = targetSize.Height;
                        if (left == 0) return input.GetSubRect(inputRect);
                    }

                    var output = new IplImage(targetSize, input.Depth, input.Channels);
                    using (var inputHeader = input.GetSubRect(inputRect))
                    {
                        CV.CopyMakeBorder(inputHeader, output, new Point(left / 2, top / 2), BorderType, FillValue);
                    }
                    return output;
                }
            });
        }
    }
}
