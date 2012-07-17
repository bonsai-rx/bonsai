using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Concurrency;

namespace Bonsai.Vision
{
    public class LoadImage : Source<IplImage>
    {
        IplImage image;
        IDisposable action;

        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public LoadImageMode Mode { get; set; }

        protected override void Start()
        {
            action = HighResolutionScheduler.ThreadPool.Schedule(() =>
            {
                Subject.OnNext(image);
                Subject.OnCompleted();
            });
        }

        protected override void Stop()
        {
            action.Dispose();
        }

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
    }
}
