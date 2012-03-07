using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public interface IWorkflowEditorService
    {
        WorkflowBuilder LoadWorkflow(string fileName);

        void OpenWorkflow(string fileName);

        Type GetTypeVisualizer(Type targetType);

        bool WorkflowRunning { get; }

        event EventHandler WorkflowStarted;

        event EventHandler WorkflowStopped;
    }
}
