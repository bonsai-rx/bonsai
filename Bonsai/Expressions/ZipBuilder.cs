using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("Zip", Namespace = Constants.XmlNamespace)]
    public class ZipBuilder : MergeCombinatorBuilder
    {
        protected override IObservable<Tuple<TSource, TOther>> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Zip(other, (xs, ys) => Tuple.Create(xs, ys));
        }
    }
}
