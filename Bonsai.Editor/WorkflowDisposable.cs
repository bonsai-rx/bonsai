using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;

namespace Bonsai.Editor
{
    class WorkflowDisposable : IDisposable
    {
        int disposed;
        IDisposable disposable;

        public WorkflowDisposable(IObservable<Unit> workflow, IDisposable disposable)
        {
            Workflow = workflow;
            this.disposable = disposable;
        }

        public IObservable<Unit> Workflow { get; private set; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                disposable.Dispose();
            }
        }
    }
}
