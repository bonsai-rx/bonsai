using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class LargestBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(ConnectedComponentCollection input)
        {
            var result = new ConnectedComponentCollection(input.ImageSize);

            ConnectedComponent largest = null;
            for (int i = 0; i < input.Count; i++)
            {
                var component = input[i];
                if (largest == null || component.Area > largest.Area)
                {
                    largest = component;
                }
            }

            if (input.Count > 0)
            {
                result.Add(largest);
            }

            return result;
        }
    }
}
