using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ConnectedComponentCollection : Collection<ConnectedComponent>
    {
        public ConnectedComponentCollection(CvSize imageSize)
        {
            ImageSize = imageSize;
        }

        public ConnectedComponentCollection(IList<ConnectedComponent> components, CvSize imageSize)
            : base(components)
        {
            ImageSize = imageSize;
        }

        public CvSize ImageSize { get; private set; }
    }
}
