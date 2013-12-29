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
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns a sequence that contains only distinct elements.")]
    public class Distinct : Combinator
    {
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Distinct();
        }
    }
}
