using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Slice : Combinator
    {
        public Slice()
        {
            Step = 1;
        }

        public int Start { get; set; }

        public int Step { get; set; }

        public int? Stop { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                int i = 0;
                return source.Where(xs =>
                {
                    var index = i++;
                    return index >= Start && (!Stop.HasValue || index < Stop) && (index - Start) % Step == 0;
                });
            });
        }
    }
}
