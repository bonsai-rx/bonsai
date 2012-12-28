using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive;

namespace Bonsai.Design
{
    public static class ControlObservable
    {
        public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Control control)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            var scheduler = new ControlScheduler(control);
            return Observable.Create<TSource>(observer =>
            {
                return source.Subscribe(
                    value => scheduler.Schedule(() => observer.OnNext(value)),
                    error => scheduler.Schedule(() => observer.OnError(error)),
                    () => scheduler.Schedule(() => observer.OnCompleted()));
            });
        }
    }
}
