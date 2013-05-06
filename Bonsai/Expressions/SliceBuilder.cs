using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Slice", Namespace = Constants.XmlNamespace)]
    public class SliceBuilder : CombinatorBuilder
    {
        public SliceBuilder()
        {
            Step = 1;
        }

        public int Start { get; set; }

        public int Step { get; set; }

        public int? Stop { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
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
