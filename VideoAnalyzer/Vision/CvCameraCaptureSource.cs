using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading;

namespace VideoAnalyzer.Vision
{
    public class CvCameraCaptureSource : Source<IplImage>
    {
        CvCapture capture;
        Thread captureThread;
        volatile bool running;
        ManualResetEventSlim stop;

        public CvCameraCaptureSource()
        {
            captureThread = new Thread(CaptureNewFrame);
            stop = new ManualResetEventSlim();
        }

        public int Index { get; set; }

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

        public override void Load(WorkflowContext context)
        {
            capture = CvCapture.CreateCameraCapture(Index);

            var width = (int)capture.GetProperty(CaptureProperty.FRAME_WIDTH);
            var height = (int)capture.GetProperty(CaptureProperty.FRAME_HEIGHT);
            context.AddService(typeof(CvSize), new CvSize(width, height));
        }

        public override void Unload()
        {
            capture.Close();
        }
    }
}
