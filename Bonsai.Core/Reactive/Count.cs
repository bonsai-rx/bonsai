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
    [Description("Returns a sequence containing an integer that represents the total number of elements in the input sequence.")]
    public class Count : Combinator<int>
    {
        public override IObservable<int> Process<TSource>(IObservable<TSource> source)
        {
            return source.Count();
        }
    }
}
