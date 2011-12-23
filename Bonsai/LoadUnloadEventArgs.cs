using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public class LoadUnloadEventArgs : EventArgs
    {
        public LoadUnloadEventArgs(WorkflowContext context)
        {
            Context = context;
        }

        public WorkflowContext Context { get; private set; }
    }
}
