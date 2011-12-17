using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class WorkflowElement
    {
        public abstract void Load(WorkflowContext context);

        public abstract void Unload(WorkflowContext context);
    }
}
