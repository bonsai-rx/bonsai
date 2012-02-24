using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Repeat", Namespace = Constants.XmlNamespace)]
    public class RepeatBuilder : CombinatorBuilder
    {
        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Repeat();
        }
    }
}
