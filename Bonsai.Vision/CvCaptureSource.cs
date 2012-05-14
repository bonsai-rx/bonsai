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
            captureThread.Join();
        }

        public override IDisposable Load()
        {
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
