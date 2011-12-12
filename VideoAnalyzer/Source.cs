using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.ComponentModel;

namespace VideoAnalyzer
{
    public abstract class Source : WorkflowElement
    {
        public abstract void Start();

        public abstract void Stop();
    }

    public abstract class Source<T> : Source
    {
        OutputObservable output;

        protected Source()
        {
            output = new OutputObservable();
        }

        [Browsable(false)]
        public IObservable<T> Output
        {
            get { return output; }
        }

        protected virtual void OnOutput(T value)
        {
            output.OnNext(value);
        }

        #region OutputObservable

        class OutputObservable : IObservable<T>
        {
            event Action<T> observers;

            public IDisposable Subscribe(IObserver<T> observer)
            {
                observers += observer.OnNext;
                return Disposable.Create(() => observers -= observer.OnNext);
            }

            public void OnNext(T output)
            {
                var handler = observers;
                if (handler != null)
                {
                    handler(output);
                }
            }
        }

        #endregion
    }
}
