using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class ConnectedComponentCheck : Condition<ConnectedComponentCollection>
    {
        public override bool Process(ConnectedComponentCollection input)
        {
            return input.Count > 0;
        }
    }
}
