using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai
{
    public class ContinuationScheduler : IScheduler
    {
        Task task;
        object gate;

        public ContinuationScheduler()
        {
            gate = new object();
            task = new Task(() => { });
            task.Start();
        }

        public DateTimeOffset Now
        {
            get { return HighResolutionScheduler.Now; }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action != null)
            {
                return this.Schedule<TState>(state, dueTime - this.Now, action);
            }
            else throw new ArgumentNullException("action");
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action != null)
            {
                var normalizedDueTime = Scheduler.Normalize(dueTime);
                if (normalizedDueTime.Ticks != 0)
                {
                    return HighResolutionScheduler.Default.Schedule<TState>(state, normalizedDueTime, (_, xs) => this.Schedule<TState>(xs, action));
                }
                else return this.Schedule<TState>(state, action);
            }
            else throw new ArgumentNullException("action");
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action != null)
            {
                var serialDisposable = new SerialDisposable();
                var cancellationDisposable = new CancellationDisposable();
                serialDisposable.Disposable = cancellationDisposable;
                lock (gate)
                {
                    task = task.ContinueWith(ts =>
                    {
                        try
                        {
                            serialDisposable.Disposable = action(this, state);
                        }
                        catch (Exception ex)
                        {
                            var exception = ex;
                            var thread = new Thread(() => { throw exception; });
                            thread.Start();
                            thread.Join();
                        }
                    }, cancellationDisposable.Token);
                }
                return serialDisposable;
            }
            else throw new ArgumentNullException("action");
        }
    }
}
