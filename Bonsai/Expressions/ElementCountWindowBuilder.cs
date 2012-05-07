using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    [XmlType("ElementCountWindow", Namespace = Constants.XmlNamespace)]
    public class ElementCountWindowBuilder : WindowBuilder
    {
        public int Count { get; set; }

        protected override IObservable<IObservable<TSource>> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Window(Count);
        }
    }
}
