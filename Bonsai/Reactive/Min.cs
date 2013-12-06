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
    public class Min
    {
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Min();
        }

        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Min();
        }

        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Min();
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Min();
        }

        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Min();
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Min();
        }

        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Min();
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Min();
        }

        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Min();
        }

        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Min();
        }

        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Min();
        }
    }
}
