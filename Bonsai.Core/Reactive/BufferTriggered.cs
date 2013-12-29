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
    [Description("Projects each element of the sequence into consecutive non-overlapping buffers.")]
    public class BufferTriggered
    {
        public IObservable<IList<TSource>> Process<TSource, TBufferBoundary>(IObservable<TSource> source, IObservable<TBufferBoundary> bufferBoundaries)
        {
            return source.Buffer(bufferBoundaries);
        }
    }
}
