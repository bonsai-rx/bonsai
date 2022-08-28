using Bonsai.Editor.GraphModel;
using System;
using System.Collections.Generic;
using System.Linq;

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
            SelectionChanged?.Invoke(this, e);
        }

        public void UpdateSelection(WorkflowGraphView selectedView)
        {
            SelectedView = selectedView ?? throw new ArgumentNullException(nameof(selectedView));
            SelectedNodes = selectedView.GraphView.SelectedNodes;
            OnSelectionChanged(EventArgs.Empty);
        }
    }
}
