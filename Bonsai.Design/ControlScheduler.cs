using System;
using System.Reactive.Concurrency;
using System.Windows.Forms;
using System.Reactive.Disposables;

namespace Bonsai.Design
{
    public class ControlScheduler : IScheduler
    {
        public ControlScheduler(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            Control = control;
        }

        public Control Control { get; private set; }

        public DateTimeOffset Now
        {
            get { return Scheduler.Now; }
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var result = new SingleAssignmentDisposable();
            Control.BeginInvoke((Action)(() =>
            {
                if (!result.IsDisposed)
                {
                    result.Disposable = action(this, state);
                }
            }));

            return result;
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var scheduledTime = Now + dueTime;
            Func<IScheduler, TState, IDisposable> scheduledAction = (scheduler, scheduledState) =>
            {
                var remainingTime = (int)Scheduler.Normalize(scheduledTime - Now).TotalMilliseconds;
                if (remainingTime > 0)
                {
                    var result = new MultipleAssignmentDisposable();
                    var timer = new Timer();
                    timer.Tick += (sender, e) =>
                    {
                        timer.Enabled = false;
                        result.Disposable = action(scheduler, scheduledState);
                    };

                    timer.Interval = remainingTime;
                    timer.Enabled = true;
                    result.Disposable = Disposable.Create(() => timer.Enabled = false);

                    return result;
                }
                else return action(scheduler, scheduledState);
            };

            if (!Control.InvokeRequired)
            {
                return scheduledAction(this, state);
            }
            else return Schedule(state, scheduledAction);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return Schedule(state, dueTime - Now, action);
        }
    }
}
