using AForge.Video;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Video
{
    [Description("Produces a sequence of images captured from the desktop screen.")]
    public class ScreenCaptureStream : VideoCapture
    {
        int frameInterval;

        [Description("The rectangle region of the screen to capture.")]
        [Editor("Bonsai.Vision.Design.IplImageOutputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect CaptureRegion { get; set; }

        [Description("The interval between each screen grab, in milliseconds.")]
        public int FrameInterval
        {
            get { return frameInterval; }
            set
            {
                frameInterval = value;
                var videoSource = (AForge.Video.ScreenCaptureStream)VideoSource;
                if (videoSource != null)
                {
                    videoSource.FrameInterval = frameInterval;
                }
            }
        }

        protected override IVideoSource CreateVideoSource()
        {
            Rectangle region;
            var captureRegion = CaptureRegion;
            if (captureRegion.Width == 0 || captureRegion.Height == 0)
            {
                region = Rectangle.Empty;
                foreach (var screen in Screen.AllScreens)
                {
                    region = Rectangle.Union(region, screen.Bounds);
                }
            }
            else region = new Rectangle(captureRegion.X, captureRegion.Y, captureRegion.Width, captureRegion.Height);
            return new AForge.Video.ScreenCaptureStream(region, frameInterval);
        }
    }
}
