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
            return Observable.Create<IplImage>(observer =>
            {
                CvArr mask;
                if (string.IsNullOrEmpty(FileName))
                {
                    mask = CvArr.Null;
                }
                else mask = HighGui.cvLoadImage(FileName, LoadImageMode.Grayscale);

                var process = source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    output.SetZero();
                    Core.cvCopy(input, output, mask);
                    return output;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    if (mask != null && mask != CvArr.Null)
                    {
                        mask.Close();
                    }
                });

                return new CompositeDisposable(process, close);
            });
        }
    }
}
