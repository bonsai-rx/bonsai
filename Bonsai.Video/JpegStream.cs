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

namespace Bonsai.Video
{
    [Description("Produces a sequence of JPEG images downloaded from the specified URL.")]
    public class JpegStream : VideoCapture
    {
        [Description("The URL which will provide JPEG image files.")]
        public string SourceUrl { get; set; }

        [Description("The login required to access the video source.")]
        public string Login { get; set; }

        [Description("The password required to access the video source.")]
        public string Password { get; set; }

        [Description("The interval between frame requests, in milliseconds.")]
        public int FrameInterval { get; set; }

        protected override IVideoSource CreateVideoSource()
        {
            return new JPEGStream(SourceUrl)
            {
                Login = Login,
                Password = Password,
                FrameInterval = FrameInterval
            };
        }
    }
}
