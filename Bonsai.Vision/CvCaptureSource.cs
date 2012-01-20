using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading;

namespace Bonsai.Vision
{
    public abstract class CvCaptureSource : Source<IplImage>
    {
        CvCapture capture;
        Thread captureThread;
        volatile bool running;
        ManualResetEventSlim stop;

        public CvCaptureSource()
        {
            stop = new ManualResetEventSlim();
        }

        void CaptureNewFrame()
        {
            while (running)
            {
                var image = capture.QueryFrame();
                if (image == null)
                {
                    Subject.OnCompleted();
                    break;
                }

                Subject.OnNext(image.Clone());
            }

            stop.Set();
        }

        protected override void Start()
        {
            running = true;
            captureThread.Start();
        }

        protected override void Stop()
        {
            running = false;
            stop.Wait();
        }

        protected abstract CvCapture CreateCapture();

        public override IDisposable Load()
        {
            captureThread = new Thread(CaptureNewFrame);
            capture = CreateCapture();
            return base.Load();
        }

        protected override void Unload()
        {
            capture.Close();
            captureThread = null;
            base.Unload();
        }
    }
}
