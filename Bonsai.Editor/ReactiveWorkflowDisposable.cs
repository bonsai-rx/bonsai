using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bonsai.Editor
{
    class ReactiveWorkflowDisposable : IDisposable
    {
        int disposed;
        IDisposable disposable;

        public ReactiveWorkflowDisposable(ReactiveWorkflow workflow, IDisposable disposable)
        {
            Workflow = workflow;
            this.disposable = disposable;
        }

        public ReactiveWorkflow Workflow { get; private set; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                disposable.Dispose();
            }
        }
    }
}
