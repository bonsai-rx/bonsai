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
    [XmlType("Sample", Namespace = Constants.XmlNamespace)]
    [Description("Samples values of the first sequence only when the second sequence produces an element.")]
    public class SampleBuilder : BinaryCombinatorBuilder
    {
        protected override IObservable<TSource> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Sample(other);
        }
    }
}
