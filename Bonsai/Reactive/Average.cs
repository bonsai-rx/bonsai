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
    [Description("Computes the average of an observable sequence.")]
    public class Average
    {
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Average();
        }

        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Average();
        }

        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Average();
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Average();
        }

        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Average();
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Average();
        }

        public IObservable<double?> Process(IObservable<int?> source)
        {
            return source.Average();
        }

        public IObservable<double> Process(IObservable<int> source)
        {
            return source.Average();
        }

        public IObservable<double?> Process(IObservable<long?> source)
        {
            return source.Average();
        }

        public IObservable<double> Process(IObservable<long> source)
        {
            return source.Average();
        }
    }
}
