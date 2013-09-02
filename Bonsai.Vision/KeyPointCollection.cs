using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class KeyPointCollection : Collection<Point2f>
    {
        public KeyPointCollection(IplImage image)
        {
            Image = image;
        }

        public IplImage Image { get; private set; }
    }
}
