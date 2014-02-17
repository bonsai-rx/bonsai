using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Design
{
    public interface IWorkflowEditorState
    {
        bool WorkflowRunning { get; }

        event EventHandler WorkflowStarted;

        event EventHandler WorkflowStopped;
    }
}
