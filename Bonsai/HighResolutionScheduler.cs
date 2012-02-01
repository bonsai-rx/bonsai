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

        public static HighResolutionCurrentThreadScheduler CurrentThread
        {
            get { return HighResolutionCurrentThreadScheduler.Instance; }
        }

        public static HighResolutionImmediateScheduler Immediate
        {
            get { return HighResolutionImmediateScheduler.Instance; }
        }

        public static HighResolutionNewThreadScheduler NewThread
        {
            get { return HighResolutionNewThreadScheduler.Instance; }
        }

        public static HighResolutionTaskPoolScheduler TaskPool
        {
            get { return HighResolutionTaskPoolScheduler.Instance; }
        }

        public static HighResolutionThreadPoolScheduler ThreadPool
        {
            get { return HighResolutionThreadPoolScheduler.Instance; }
        }
    }

    public abstract class StopwatchScheduler : IScheduler
    {
        IScheduler scheduler;

        internal StopwatchScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public DateTimeOffset Now
        {
            get
            {
                return HighResolutionScheduler.Now;
            }
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return scheduler.Schedule(state, action);
        }
    }

    public sealed class HighResolutionCurrentThreadScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionCurrentThreadScheduler Instance = new HighResolutionCurrentThreadScheduler();

        public HighResolutionCurrentThreadScheduler()
            : base(Scheduler.CurrentThread)
        {
        }
    }

    public sealed class HighResolutionImmediateScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionImmediateScheduler Instance = new HighResolutionImmediateScheduler();

        public HighResolutionImmediateScheduler()
            : base(Scheduler.Immediate)
        {
        }
    }

    public sealed class HighResolutionNewThreadScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionNewThreadScheduler Instance = new HighResolutionNewThreadScheduler();

        public HighResolutionNewThreadScheduler()
            : base(Scheduler.NewThread)
        {
        }
    }

    public sealed class HighResolutionTaskPoolScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionTaskPoolScheduler Instance = new HighResolutionTaskPoolScheduler();

        public HighResolutionTaskPoolScheduler()
            : base(Scheduler.TaskPool)
        {
        }
    }

    public sealed class HighResolutionThreadPoolScheduler : StopwatchScheduler
    {
        internal static readonly HighResolutionThreadPoolScheduler Instance = new HighResolutionThreadPoolScheduler();

        public HighResolutionThreadPoolScheduler()
            : base(Scheduler.ThreadPool)
        {
        }
    }
}
