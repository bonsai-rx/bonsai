using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    public struct EdgeDescriptor<TLabel>
    {
        public EdgeDescriptor(int from, int to, TLabel label)
            : this()
        {
            From = from;
            To = to;
            Label = label;
        }

        public int From { get; set; }

        public int To { get; set; }

        public TLabel Label { get; set; }
    }
}
