using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Gate", Namespace = Constants.XmlNamespace)]
    [Description("Allows the next value of the first sequence to propagate only when the second sequence produces an element.")]
    public class GateBuilder : BinaryCombinatorBuilder
    {
        protected override IObservable<TSource> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other);
        }
    }
}
