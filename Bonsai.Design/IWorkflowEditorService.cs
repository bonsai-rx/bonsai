using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Design
{
    public interface IWorkflowEditorService
    {
        WorkflowBuilder LoadWorkflow(string fileName);

        void OpenWorkflow(string fileName);

        void StoreWorkflowElements(WorkflowBuilder builder);

        WorkflowBuilder RetrieveWorkflowElements();

        IEnumerable<Type> GetTypeVisualizers(Type targetType);

        void StartWorkflow();

        void StopWorkflow();

        bool WorkflowRunning { get; }

        event EventHandler WorkflowStarted;

        event EventHandler WorkflowStopped;

        void Undo();

        void Redo();
    }
}
