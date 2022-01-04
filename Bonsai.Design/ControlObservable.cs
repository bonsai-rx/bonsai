using System;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a set of static methods for subscribing to observable sequences using Windows Forms controls.
    /// </summary>
    public static class ControlObservable
    {
        /// <summary>
        /// Wraps the source sequence in order to run its observer callbacks in the UI thread of the
        /// specified control.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// The observable sequence whose notifications will be scheduled in the UI thread of the
        /// specified control.
        /// </param>
        /// <param name="control">
        /// A <see cref="Control"/> object whose underlying handle will be used to schedule notifications.
        /// </param>
        /// <returns>
        /// An observable sequence with the same elements as the <paramref name="source"/> sequence,
        /// but where all notifications will be raised in the UI thread of the specified
        /// <paramref name="control"/>.
        /// </returns>
        public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Control control)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
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
