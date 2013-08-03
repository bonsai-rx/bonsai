using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Combinators
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Propagates only the specified number of contiguous elements from the end of the sequence.")]
    public class TakeLast : Combinator
    {
        [Description("The number of elements to propagate.")]
        public int Count { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.TakeLast(Count);
        }
    }
}
