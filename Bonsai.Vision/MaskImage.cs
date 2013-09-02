using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class MaskImage : Transform<IplImage, IplImage>
    {
        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Arr mask;
                if (string.IsNullOrEmpty(FileName))
                {
                    mask = null;
                }
                else mask = CV.LoadImage(FileName, LoadImageFlags.Grayscale);
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    output.SetZero();
                    CV.Copy(input, output, mask);
                    return output;
                });
            });
        }
    }
}
