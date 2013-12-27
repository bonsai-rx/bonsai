using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [TypeConverter("Bonsai.Vision.Design.CapturePropertyCollectionConverter, Bonsai.Vision.Design")]
    public class CapturePropertyCollection : Collection<CapturePropertyAssignment>
    {
        public Capture Capture { get; internal set; }
    }
}
