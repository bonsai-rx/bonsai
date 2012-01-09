using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading;

namespace Bonsai.Vision
{
    public class CameraCapture : CvCaptureSource
    {
        public int Index { get; set; }

        protected override CvCapture CreateCapture()
        {
            return CvCapture.CreateCameraCapture(Index);
        }
    }
}
