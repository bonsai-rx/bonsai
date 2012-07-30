using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    public class LoadImage : Source<IplImage>
    {
        IplImage image;

        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public LoadImageMode Mode { get; set; }

        public override IDisposable Load()
        {
            image = HighGui.cvLoadImage(FileName, Mode);
            return base.Load();
        }

        protected override void Unload()
        {
            image.Dispose();
            image = null;
            base.Unload();
        }

        protected override IObservable<IplImage> Generate()
        {
            return Observable.Return(image);
        }
    }
}
