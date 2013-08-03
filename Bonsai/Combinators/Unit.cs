using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Reactive;

namespace Bonsai.Combinators
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Converts a sequence of any type into a sequence of Unit type elements.")]
    public class Unit : Combinator<System.Reactive.Unit>
    {
        public override IObservable<System.Reactive.Unit> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(xs => System.Reactive.Unit.Default);
        }
    }
}
