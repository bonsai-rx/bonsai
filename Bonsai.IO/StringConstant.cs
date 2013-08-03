using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    public class StringConstant : Combinator<string>
    {
        public string Literal { get; set; }

        public override IObservable<string> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Literal);
        }
    }
}
