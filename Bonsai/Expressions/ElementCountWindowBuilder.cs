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
    [Description("Projects the sequence into non-overlapping windows with the specified maximum number of elements.")]
    public class ElementCountWindowBuilder : WindowBuilder
    {
        [Description("The maximum number of elements in each window.")]
        public int Count { get; set; }

        protected override IObservable<IObservable<TSource>> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Window(Count);
        }
    }
}
