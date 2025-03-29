using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.Editor
{
    class WorkflowDisposable : IDisposable
    {
        int disposed;
        readonly IDisposable disposable;

        public WorkflowDisposable(IObservable<Unit> workflow, Action dispose)
        {
            Workflow = workflow;
            disposable = Disposable.Create(dispose);
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
