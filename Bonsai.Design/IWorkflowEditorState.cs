using System;

namespace Bonsai.Design
{
    public interface IWorkflowEditorState
    {
        bool WorkflowRunning { get; }

        event EventHandler WorkflowStarted;

        event EventHandler WorkflowStopped;
    }
}
