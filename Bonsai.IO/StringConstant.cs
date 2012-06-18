using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    public class StringConstant : CombinatorBuilder<string>
    {
        public string Literal { get; set; }

        protected override IObservable<string> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Literal);
        }
    }
}
