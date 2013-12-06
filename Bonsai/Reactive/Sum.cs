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
    public class Sum
    {
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Sum();
        }

        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Sum();
        }

        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Sum();
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Sum();
        }

        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Sum();
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Sum();
        }

        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Sum();
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Sum();
        }

        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Sum();
        }

        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Sum();
        }
    }
}
