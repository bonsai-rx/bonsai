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
    [Description("Ignores all elements in a sequence leaving only the termination messages.")]
    public class IgnoreElements : Combinator
    {
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.IgnoreElements();
        }
    }
}
