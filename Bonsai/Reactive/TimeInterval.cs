using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Records the time interval between consecutive values produced by the sequence.")]
    public class TimeInterval
    {
        public IObservable<System.Reactive.TimeInterval<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.TimeInterval(HighResolutionScheduler.Default);
        }
    }
}
