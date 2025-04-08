using System;
using Bonsai.Editor.GraphModel;

namespace Bonsai.Editor
{
    delegate void WorkflowNavigateEventHandler(object sender, WorkflowNavigateEventArgs e);

    class WorkflowNavigateEventArgs : EventArgs
    {
        public WorkflowNavigateEventArgs(
            WorkflowEditorPath workflowPath,
            NavigationPreference navigationPreference = NavigationPreference.Current)
        {
            WorkflowPath = workflowPath;
            NavigationPreference = navigationPreference;
        }

        public WorkflowEditorPath WorkflowPath { get; }

        public NavigationPreference NavigationPreference { get; }
    }
}
