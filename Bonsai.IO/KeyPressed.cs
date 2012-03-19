using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Vision.AForge
{
    public class KeyPressed : Filter<Keys>
    {
        public Keys Key { get; set; }

        public override bool Process(Keys input)
        {
            return input == Key;
        }
    }
}
