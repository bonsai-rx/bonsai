using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    public class ConnectedComponent
    {
        public CvPoint2D32f Centroid { get; set; }

        public double Orientation { get; set; }

        public double MajorAxisLength { get; set; }

        public double MinorAxisLength { get; set; }

        public double Area { get; set; }

        [XmlIgnore]
        public CvContour Contour { get; set; }
    }
}
