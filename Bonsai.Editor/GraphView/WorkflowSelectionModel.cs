using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Editor.GraphView
{
    class WorkflowSelectionModel
    {
        public WorkflowSelectionModel()
        {
            SelectedNodes = Enumerable.Empty<GraphNode>();
        }

        public event EventHandler SelectionChanged;

        public WorkflowGraphView SelectedView { get; private set; }

        public IEnumerable<GraphNode> SelectedNodes { get; private set; }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void UpdateSelection(WorkflowGraphView selectedView)
        {
            if (selectedView == null)
            {
                throw new ArgumentNullException("selectedView");
            }

            SelectedView = selectedView;
            SelectedNodes = selectedView.GraphView.SelectedNodes;
            OnSelectionChanged(EventArgs.Empty);
        }
    }
}
