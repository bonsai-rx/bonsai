using System;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides information about the state of the workflow editor.
    /// </summary>
    public interface IWorkflowEditorState
    {
        /// <summary>
        /// Gets a value indicating whether the workflow is running.
        /// </summary>
        bool WorkflowRunning { get; }

        /// <summary>
        /// Occurs when the workflow starts.
        /// </summary>
        event EventHandler WorkflowStarted;

        /// <summary>
        /// Occurs when the workflow stops.
        /// </summary>
        event EventHandler WorkflowStopped;
    }
}
