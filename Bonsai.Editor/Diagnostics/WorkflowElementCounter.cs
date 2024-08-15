using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Bonsai.Expressions;

namespace Bonsai.Editor.Diagnostics
{
    internal class WorkflowElementCounter : IDisposable
    {
        readonly IDisposable subscription;
        private long subscribeCount;
        private long onNextCount;
        private long onCompletedCount;
        private long onErrorCount;
        private long disposeCount;
        private long lastNotificationCount;

        public WorkflowElementCounter(InspectBuilder inspectBuilder)
        {
            InspectBuilder = inspectBuilder ?? throw new ArgumentNullException(nameof(inspectBuilder));
            subscription = InspectBuilder.Output.SelectMany(source =>
            {
                Interlocked.Increment(ref subscribeCount);
                return source
                    .Do(value => Interlocked.Increment(ref onNextCount),
                        error => Interlocked.Increment(ref onErrorCount),
                        () => Interlocked.Increment(ref onCompletedCount))
                    .IgnoreElements()
                    .Finally(() => Interlocked.Increment(ref disposeCount));
            }).Subscribe();
        }

        public InspectBuilder InspectBuilder { get; }

        public long SubscribeCount => subscribeCount;

        public long OnNextCount => onNextCount;

        public long OnErrorCount => onErrorCount;

        public long OnCompletedCount => onCompletedCount;

        public long DisposeCount => disposeCount;

        public long TotalCount =>
            Interlocked.Read(ref subscribeCount) +
            Interlocked.Read(ref onNextCount) +
            Interlocked.Read(ref onErrorCount) +
            Interlocked.Read(ref onCompletedCount) +
            Interlocked.Read(ref disposeCount);

        public WorkflowElementStatus GetStatus()
        {
            var notificationCount = Interlocked.Read(ref onNextCount);
            var activeSubscriptions = Interlocked.Read(ref subscribeCount) - Interlocked.Read(ref disposeCount);
            if (activeSubscriptions > 0)
            {
                if (notificationCount > lastNotificationCount)
                {
                    lastNotificationCount = notificationCount;
                    return WorkflowElementStatus.Notifying;
                }

                return WorkflowElementStatus.Active;
            }
            else
            {
                lastNotificationCount = notificationCount;
                var errorCount = Interlocked.Read(ref onErrorCount);
                var completedCount = Interlocked.Read(ref onCompletedCount);
                if (errorCount > 0) return WorkflowElementStatus.Error;
                else if (completedCount > 0) return WorkflowElementStatus.Completed;
                return WorkflowElementStatus.Ready;
            }
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
