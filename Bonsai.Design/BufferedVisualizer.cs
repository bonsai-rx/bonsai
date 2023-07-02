using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for type visualizers with an update
    /// frequency potentially much higher than the screen refresh rate.
    /// </summary>
    public abstract class BufferedVisualizer : DialogTypeVisualizer
    {
        const int TargetInterval = 1000 / 50;

        internal BufferedVisualizer()
        {
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            if (provider.GetService(typeof(IDialogTypeVisualizerService)) is not Control visualizerControl)
            {
                return source;
            }

            return Observable.Using(
                () => new Timer(),
                timer =>
                {
                    timer.Interval = TargetInterval;
                    var timerTick = Observable.FromEventPattern<EventHandler, EventArgs>(
                        handler => timer.Tick += handler,
                        handler => timer.Tick -= handler);
                    timer.Start();
                    var mergedSource = source.SelectMany(xs => xs.Do(
                        _ => { },
                        () => visualizerControl.BeginInvoke((Action)SequenceCompleted)));
                    return mergedSource
                        .Timestamp(HighResolutionScheduler.Default)
                        .Buffer(() => timerTick)
                        .Do(ShowBuffer).Finally(timer.Stop);
                });
        }

        /// <summary>
        /// Updates the type visualizer with a new buffer of timestamped values.
        /// </summary>
        /// <param name="values">
        /// A buffer of timestamped values where each timestamp indicates the
        /// time at which the value was received.
        /// </param>
        protected virtual void ShowBuffer(IList<Timestamped<object>> values)
        {
            foreach (var timestamped in values)
            {
                var time = timestamped.Timestamp.LocalDateTime;
                Show(time, timestamped.Value);
            }
        }

        /// <summary>
        /// Updates the type visualizer to display a buffered value object
        /// received at the specified time.
        /// </summary>
        /// <param name="time">The time at which the value was received.</param>
        /// <param name="value">The value to visualize.</param>
        protected virtual void Show(DateTime time, object value)
        {
            Show(value);
        }
    }
}
