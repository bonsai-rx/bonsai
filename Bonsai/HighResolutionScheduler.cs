using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Diagnostics;

namespace Bonsai
{
    /// <summary>
    /// Provides a set of static properties to access schedulers that use the
    /// <see cref="Stopwatch"/> class for generating timestamps.
    /// </summary>
    public static class HighResolutionScheduler
    {
        static readonly double Frequency = Stopwatch.IsHighResolution ? TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency + .0001245 : 1;
        static readonly double NowOffset = Stopwatch.IsHighResolution ? DateTime.Now.Ticks - Stopwatch.GetTimestamp() * Frequency : 0;

        /// <summary>
        /// Gets the current time according to the timer used by the <see cref="Stopwatch"/> class.
        /// </summary>
        public static DateTimeOffset Now
        {
            get
            {
                return new DateTimeOffset(new DateTime(
                    (long)(Stopwatch.GetTimestamp() * Frequency + NowOffset),
                    DateTimeKind.Local));
            }
        }

        /// <summary>
        /// Gets a scheduler that schedules work on the platform's default scheduler
        /// but provides high resolution timestamps.
        /// </summary>
        public static IScheduler Default
        {
            get { return HighResolutionDefaultScheduler.Default; }
        }
    }

    abstract class StopwatchScheduler : LocalScheduler
    {
        IScheduler scheduler;

        internal StopwatchScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public override DateTimeOffset Now
        {
            get
            {
                return HighResolutionScheduler.Now;
            }
        }

        public override IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, action);
        }
    }

    sealed class HighResolutionDefaultScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionDefaultScheduler Default = new HighResolutionDefaultScheduler();

        public HighResolutionDefaultScheduler()
            : base(Scheduler.Default)
        {
        }
    }
}
