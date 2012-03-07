using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class WorkflowSelectionModel
    {
        public event EventHandler SelectionChanged;

        public WorkflowViewModel SelectedModel { get; private set; }

        public GraphNode SelectedNode { get; private set; }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void SetSelectedNode(WorkflowViewModel workflowViewModel, GraphNode selectedNode)
        {
            SelectedModel = workflowViewModel;
            SelectedNode = selectedNode;
            OnSelectionChanged(EventArgs.Empty);
        }
    }
}
