using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Bonsai.Vision
{
    public class CameraCapture : Source<IplImage>
    {
        CvCapture capture;

        public int Index { get; set; }

        public override IDisposable Load()
        {
            capture = CvCapture.CreateCameraCapture(Index);
            return base.Load();
        }

        protected override void Unload()
        {
            capture.Close();
            base.Unload();
        }

        protected override IObservable<IplImage> Generate()
        {
            return Observable.Create<IplImage>(observer => HighResolutionScheduler.TaskPool.Schedule(loop =>
            {
                var image = capture.QueryFrame();
                if (image == null) observer.OnCompleted();
                else
                {
                    observer.OnNext(image.Clone());
                    loop();
                }
            }));
        }
    }
}
