using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("TakeUntil", Namespace = Constants.XmlNamespace)]
    [Description("Propagates values from the first sequence only until the second sequence produces a value.")]
    public class TakeUntilBuilder : BinaryCombinatorBuilder
    {
        protected override IObservable<TSource> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.TakeUntil(other);
        }
    }
}
