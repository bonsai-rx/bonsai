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
                    var area = ImgProc.cvContourArea(component.Contour, CvSlice.WholeSeq, 0);
                    if (area > max)
                    {
                        largest = component;
                        max = area;
                    }
                }

                return largest;
            }
        }

        public CvSize ImageSize { get; private set; }
    }
}
