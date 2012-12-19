using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class LargestBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponent>
    {
        public override ConnectedComponent Process(ConnectedComponentCollection input)
        {
            ConnectedComponent largest = new ConnectedComponent();
            for (int i = 0; i < input.Count; i++)
            {
                var component = input[i];
                if (component.Area > largest.Area)
                {
                    largest = component;
                }
            }

            return largest;
        }
    }
}
