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
                if (image == null) break;
                OnOutput(image);
            }

            stop.Set();
        }

        public override void Start()
        {
            running = true;
            captureThread.Start();
        }

        public override void Stop()
        {
            running = false;
            stop.Wait();
        }

        protected abstract CvCapture CreateCapture(WorkflowContext context);

        public override void Load(WorkflowContext context)
        {
            captureThread = new Thread(CaptureNewFrame);
            capture = CreateCapture(context);

            var width = (int)capture.GetProperty(CaptureProperty.FRAME_WIDTH);
            var height = (int)capture.GetProperty(CaptureProperty.FRAME_HEIGHT);
            context.AddService(typeof(CvSize), new CvSize(width, height));
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            capture.Close();
            captureThread = null;
            context.RemoveService(typeof(CvSize));
            base.Unload(context);
        }
    }
}
