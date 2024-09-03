using System;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Expressions;

namespace Bonsai.Editor.Diagnostics
{
    internal class WorkflowElementCounter : IDisposable
    {
        readonly IDisposable subscription;
        private NotificationCounter subscribeCounter;
        private NotificationCounter onNextCounter;
        private NotificationCounter onCompletedCounter;
        private NotificationCounter onErrorCounter;
        private NotificationCounter unsubscribeCounter;
        private WorkflowElementStatus lastInactiveStatus;

        public WorkflowElementCounter(InspectBuilder inspectBuilder)
        {
            InspectBuilder = inspectBuilder ?? throw new ArgumentNullException(nameof(inspectBuilder));
            subscription = InspectBuilder.Watch.SelectMany(source => source.Do(notification =>
            {
                switch (notification)
                {
                    case WatchNotification.Subscribe: subscribeCounter.Increment(); break;
                    case WatchNotification.OnNext: onNextCounter.Increment(); break;
                    case WatchNotification.OnError: onErrorCounter.Increment(); break;
                    case WatchNotification.OnCompleted: onCompletedCounter.Increment(); break;
                    case WatchNotification.Unsubscribe: unsubscribeCounter.Increment(); break;
                }
            }).IgnoreElements()).Subscribe();
        }

        public InspectBuilder InspectBuilder { get; }

        public long SubscribeCount => subscribeCounter.Read();

        public long OnNextCount => onNextCounter.Read();

        public long OnErrorCount => onErrorCounter.Read();

        public long OnCompletedCount => onCompletedCounter.Read();

        public long UnsubscribeCount => unsubscribeCounter.Read();

        public long TotalCount =>
            SubscribeCount +
            OnNextCount +
            OnErrorCount +
            OnCompletedCount +
            UnsubscribeCount;

        public WorkflowElementStatus GetStatus()
        {
            var notificationDelta = onNextCounter.ReadDelta(out long notificationCount);
            var unsubscribeDelta = unsubscribeCounter.ReadDelta(out long unsubscribeCount);
            var activeSubscriptions = SubscribeCount - unsubscribeCount;
            if (activeSubscriptions > 0)
            {
                return notificationDelta > 0
                    ? WorkflowElementStatus.Notifying
                    : WorkflowElementStatus.Active;
            }
            else if (unsubscribeDelta > 0)
            {
                var errorDelta = onErrorCounter.ReadDelta(out long _);
                var completedDelta = onCompletedCounter.ReadDelta(out long _);
                if (errorDelta > 0) lastInactiveStatus = WorkflowElementStatus.Error;
                else if (completedDelta > 0) lastInactiveStatus = WorkflowElementStatus.Completed;
                else lastInactiveStatus = WorkflowElementStatus.Canceled;
            }

            return lastInactiveStatus;
        }

        public void Dispose()
        {
            subscription.Dispose();
        }
    }
}
