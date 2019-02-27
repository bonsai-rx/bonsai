using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    class WriterDisposable<TWriter> : IDisposable where TWriter : IDisposable
    {
        readonly EventLoopScheduler scheduler;

        public WriterDisposable()
            : this(true)
        {
        }

        public WriterDisposable(bool buffered)
        {
            scheduler = buffered ? new EventLoopScheduler() : null;
        }

        public TWriter Writer { get; set; }

        public void Schedule(Action action)
        {
            if (scheduler == null) action();
            else scheduler.Schedule(action);
        }

        void DisposeInternal()
        {
            var writer = Writer;
            if (writer != null)
            {
                writer.Dispose();
            }
        }

        public void Dispose()
        {
            if (scheduler == null) DisposeInternal();
            else
            {
                scheduler.Schedule(() =>
                {
                    DisposeInternal();
                    scheduler.Dispose();
                });
            }
        }
    }
}
