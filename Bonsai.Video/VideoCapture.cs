using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using System.Drawing;
using OpenCV.Net;
using System.Drawing.Imaging;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Video
{
    public abstract class VideoCapture : Source<IplImage>
    {
        IObservable<IplImage> source;

        public VideoCapture()
        {
            source = Observable.Create<IplImage>(observer =>
            {
                var videoSource = CreateVideoSource();
                videoSource.NewFrame += (sender, e) => observer.OnNext(ProcessFrame(e.Frame));
                videoSource.VideoSourceError += (sender, e) => observer.OnError(new VideoException(e.Description));
                videoSource.PlayingFinished += (sender, e) => observer.OnCompleted();
                videoSource.Start();
                VideoSource = videoSource;
                return new CompositeDisposable
                {
                    Disposable.Create(() => VideoSource = null),
                    Disposable.Create(videoSource.SignalToStop)
                };
            })
            .PublishReconnectable()
            .RefCount();
        }

        [XmlIgnore]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IVideoSource VideoSource { get; private set; }

        protected abstract IVideoSource CreateVideoSource();

        protected virtual IplImage ProcessFrame(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var image = new IplImage(new OpenCV.Net.Size(bitmap.Width, bitmap.Height), IplDepth.U8, 3, bitmapData.Scan0);
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                CV.Copy(image, output);
                return output;
            }
            finally { bitmap.UnlockBits(bitmapData); }
        }

        public override IObservable<IplImage> Generate()
        {
            return source;
        }
    }
}
