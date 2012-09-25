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
    [XmlType("RepeatCount", Namespace = Constants.XmlNamespace)]
    [Description("Repeats the sequence a definite number of times.")]
    public class RepeatCountBuilder : CombinatorBuilder
    {
        [Description("The number of times the sequence should be repeated.")]
        public int Count { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Repeat(Count);
        }
    }
}
