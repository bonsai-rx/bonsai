using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Skip", Namespace = Constants.XmlNamespace)]
    [Description("Bypasses the specified number of contiguous elements at the start of the sequence.")]
    public class SkipBuilder : CombinatorBuilder
    {
        [Description("The number of elements to skip.")]
        public int Count { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Skip(Count);
        }
    }
}
