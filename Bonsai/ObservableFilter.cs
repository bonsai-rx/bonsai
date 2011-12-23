using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai
{
    public class ObservableFilter<T> : Filter<T, T>
    {
        OutputObservable<T> output;

        public ObservableFilter()
        {
            output = new OutputObservable<T>();
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return output; }
        }

        public override T Process(T input)
        {
            output.OnNext(input);
            return input;
        }
    }
}
