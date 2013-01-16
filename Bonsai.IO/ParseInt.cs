using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.IO
{
    public class ParseInt : Transform<string, int>
    {
        public override int Process(string input)
        {
            return int.Parse(input);
        }
    }
}
