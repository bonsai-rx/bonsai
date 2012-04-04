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

        protected abstract CvCapture CreateCapture();

        protected override void Start()
        {
            running = true;
            captureThread = new Thread(CaptureNewFrame);
            captureThread.Start();
        }

        protected override void Stop()
        {
            running = false;
            stop.Wait();
        }

        public override IDisposable Load()
        {
            stop = new ManualResetEventSlim();
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
