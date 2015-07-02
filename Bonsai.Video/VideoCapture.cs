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
using System.Threading.Tasks;
using System.Threading;

namespace Bonsai.Video
{
    public abstract class VideoCapture : Source<IplImage>
    {
        IObservable<IplImage> source;
        readonly object captureLock = new object();

        public VideoCapture()
        {
            source = Observable.Create<IplImage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    lock (captureLock)
                    {
                        // When we finally acquire the lock, do we still want to start?
                        if (cancellationToken.IsCancellationRequested) return;

                        var frame = default(IplImage);
                        var exception = default(Exception);
                        var videoSource = CreateVideoSource();
                        using (var waitEvent = new AutoResetEvent(false))
                        {
                            videoSource.NewFrame += (sender, e) =>
                            {
                                Interlocked.Exchange(ref frame, ProcessFrame(e.Frame));
                                waitEvent.Set();
                            };

                            videoSource.VideoSourceError += (sender, e) =>
                            {
                                Interlocked.Exchange(ref exception, new VideoException(e.Description));
                                waitEvent.Set();
                            };

                            videoSource.PlayingFinished += (sender, e) =>
                            {
                                Interlocked.Exchange(ref frame, null);
                                waitEvent.Set();
                            };

                            videoSource.Start();
                            VideoSource = videoSource;
                            using (var cleanUp = Disposable.Create(() => VideoSource = null))
                            using (var stopNotification = Disposable.Create(videoSource.WaitForStop))
                            using (var notification = cancellationToken.Register(videoSource.SignalToStop))
                            {
                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    waitEvent.WaitOne();
                                    if (exception != null)
                                    {
                                        observer.OnError(exception);
                                        break;
                                    }

                                    var image = frame;
                                    if (image == null)
                                    {
                                        observer.OnCompleted();
                                        break;
                                    }
                                    else observer.OnNext(image);
                                }
                            }
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
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
