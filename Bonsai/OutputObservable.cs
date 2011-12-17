using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Bonsai
{
    class OutputObservable<T> : IObservable<T>
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
}
