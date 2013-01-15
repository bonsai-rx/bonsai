using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Concurrency;
using System.Diagnostics;

namespace Bonsai
{
    public static class HighResolutionScheduler
    {
        static readonly double Frequency = Stopwatch.IsHighResolution ? TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency + .0001245 : 1;
        static readonly double NowOffset = Stopwatch.IsHighResolution ? DateTime.Now.Ticks - Stopwatch.GetTimestamp() * Frequency : 0;

        public static DateTimeOffset Now
        {
            get
            {
                return new DateTimeOffset(new DateTime(
                    (long)(Stopwatch.GetTimestamp() * Frequency + NowOffset),
                    DateTimeKind.Local));
            }
        }

        public static IScheduler Default
        {
            get { return HighResolutionDefaultScheduler.Default; }
        }
    }

    public abstract class StopwatchScheduler : LocalScheduler
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
