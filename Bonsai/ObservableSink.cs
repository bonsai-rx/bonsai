using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai
{
    public class ObservableSink<T> : Sink<T>
    {
        OutputObservable<T> output;

        public ObservableSink()
        {
            output = new OutputObservable<T>();
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return output; }
        }

        public override void Process(T input)
        {
            output.OnNext(input);
        }
    }
}
