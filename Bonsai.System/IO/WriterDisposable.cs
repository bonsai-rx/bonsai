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
        readonly EventLoopScheduler scheduler = new EventLoopScheduler();

        public TWriter Writer { get; set; }

        public EventLoopScheduler Scheduler
        {
            get { return scheduler; }
        }

        public void Dispose()
        {
            scheduler.Schedule(() =>
            {
                var writer = Writer;
                if (writer != null)
                {
                    writer.Dispose();
                }
                scheduler.Dispose();
            });
        }
    }
}
