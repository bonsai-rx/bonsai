using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("SkipLast", Namespace = Constants.XmlNamespace)]
    public class SkipLastBuilder : CombinatorBuilder
    {
        public int Count { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.SkipLast(Count);
        }
    }
}
