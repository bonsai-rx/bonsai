using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    public class ToggleSwitch : Combinator
    {
        public bool Enabled { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Where(xs => Enabled);
        }
    }
}
