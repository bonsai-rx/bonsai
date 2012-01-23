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

        public ConnectedComponent Largest
        {
            get
            {
                var max = 0.0;
                ConnectedComponent largest = null;
                foreach (var component in Items)
                {
                    if (component.Area > max)
                    {
                        largest = component;
                        max = component.Area;
                    }
                }

                return largest;
            }
        }

        public CvSize ImageSize { get; private set; }
    }
}
