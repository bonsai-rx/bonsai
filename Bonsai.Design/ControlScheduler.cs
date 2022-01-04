using System;
using System.Reactive.Concurrency;
using System.Windows.Forms;
using System.Reactive.Disposables;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents an object that schedules units of work using the UI thread
    /// of a Windows Forms control.
    /// </summary>
    public class ControlScheduler : IScheduler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlScheduler"/> class
        /// using the specified control.
        /// </summary>
        /// <param name="control">
        /// A <see cref="Control"/> object whose underlying handle will be used to
        /// schedule units of work.
        /// </param>
        public ControlScheduler(Control control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
        }

        /// <summary>
        /// Gets the control object used to schedule units of work.
        /// </summary>
        public Control Control { get; private set; }

        /// <summary>
        /// Gets the current time according to the local machine's system clock.
        /// </summary>
        public DateTimeOffset Now
        {
            get { return Scheduler.Now; }
        }

        /// <inheritdoc/>
        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
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

        /// <inheritdoc/>
        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
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

        /// <inheritdoc/>
        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Schedule(state, dueTime - Now, action);
        }
    }
}
