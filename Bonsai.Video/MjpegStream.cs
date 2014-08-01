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
    [Description("Produces a sequence of images downloaded from the specified MJPEG stream.")]
    public class MjpegStream : VideoCapture
    {
        [Description("The URL which will provide the MJPEG stream.")]
        public string SourceUrl { get; set; }

        [Description("The login required to access the video source.")]
        public string Login { get; set; }

        [Description("The password required to access the video source.")]
        public string Password { get; set; }

        protected override IVideoSource CreateVideoSource()
        {
            return new MJPEGStream(SourceUrl)
            {
                Login = Login,
                Password = Password
            };
        }
    }
}
