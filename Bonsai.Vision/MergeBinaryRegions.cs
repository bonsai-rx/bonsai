using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class MergeBinaryRegions : Transform<ConnectedComponentCollection, ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(ConnectedComponentCollection first, ConnectedComponentCollection second)
        {
            var output = new ConnectedComponentCollection(first.Concat(second).ToList(), first.ImageSize);
            return output;
        }
    }
}
