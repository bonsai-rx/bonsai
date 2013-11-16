using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.IO
{
    public class KeyPressed : Condition<Keys>
    {
        public Keys Key { get; set; }

        public override IObservable<bool> Process(IObservable<Keys> source)
        {
            return source.Select(input => input == Key);
        }
    }
}
