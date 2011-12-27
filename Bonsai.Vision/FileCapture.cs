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
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string FileName { get; set; }

        protected override CvCapture CreateCapture(WorkflowContext context)
        {
            return CvCapture.CreateFileCapture(FileName);
        }
    }
}
