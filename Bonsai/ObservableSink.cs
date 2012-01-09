using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using System.ComponentModel;

namespace Bonsai
{
    public class ObservableSink<T> : Sink<T>
    {
        Subject<T> subject = new Subject<T>();

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return subject; }
        }

        public override void Process(T input)
        {
            subject.OnNext(input);
        }
    }
}
