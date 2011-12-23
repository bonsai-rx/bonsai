using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public interface IWorkflowContainer
    {
        WorkflowElementCollection Components { get; }
    }
}
