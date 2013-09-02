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
        public ConnectedComponentCollection(Size imageSize)
        {
            ImageSize = imageSize;
        }

        public ConnectedComponentCollection(IList<ConnectedComponent> components, Size imageSize)
            : base(components)
        {
            ImageSize = imageSize;
        }

        public Size ImageSize { get; private set; }
    }
}
