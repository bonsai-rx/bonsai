using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    public class ToggleSwitch : CombinatorBuilder
    {
        public bool Enabled { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Where(xs => Enabled);
        }
    }
}
