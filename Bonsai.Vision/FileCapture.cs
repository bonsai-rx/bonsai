using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class FileCapture : CvCaptureSource
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        protected override CvCapture CreateCapture()
        {
            return CvCapture.CreateFileCapture(FileName);
        }
    }
}
