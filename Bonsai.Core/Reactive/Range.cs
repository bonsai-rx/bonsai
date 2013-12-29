using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Generates an observable sequence of integral numbers within a specified range.")]
    public class Range : Source<int>
    {
        public int Start { get; set; }

        public int Count { get; set; }

        public override IObservable<int> Generate()
        {
            return Observable.Range(Start, Count);
        }
    }
}
