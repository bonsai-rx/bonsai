using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Take", Namespace = Constants.XmlNamespace)]
    [Description("Propagates only the specified number of contiguous elements from the start of the sequence.")]
    public class TakeBuilder : CombinatorBuilder
    {
        [Description("The number of elements to propagate.")]
        public int Count { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Take(Count);
        }
    }
}
