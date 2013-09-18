using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more buffers based on element count information.")]
    public class Buffer
    {
        public int Count { get; set; }

        public int Skip { get; set; }

        public IObservable<IList<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Buffer(Count, Skip);
        }
    }
}
