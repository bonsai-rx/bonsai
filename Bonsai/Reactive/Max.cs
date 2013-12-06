using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the sum of an observable sequence.")]
    public class Max
    {
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Max();
        }

        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Max();
        }

        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Max();
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Max();
        }

        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Max();
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Max();
        }

        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Max();
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Max();
        }

        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Max();
        }

        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Max();
        }

        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Max();
        }
    }
}
