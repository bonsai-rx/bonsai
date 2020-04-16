using OpenCV.Net;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [TypeConverter("Bonsai.Vision.Design.CapturePropertyCollectionConverter, Bonsai.Vision.Design")]
    public class CapturePropertyCollection : Collection<CapturePropertyAssignment>
    {
        public Capture Capture { get; internal set; }
    }
}
