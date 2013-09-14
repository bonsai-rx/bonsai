using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class WorkflowSelectionModel
    {
        public WorkflowSelectionModel()
        {
            SelectedNodes = Enumerable.Empty<GraphNode>();
        }

        public event EventHandler SelectionChanged;

        public WorkflowViewModel SelectedModel { get; private set; }

        public IEnumerable<GraphNode> SelectedNodes { get; private set; }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void UpdateSelection(WorkflowViewModel selectedModel)
        {
            if (selectedModel == null)
            {
                throw new ArgumentNullException("selectedModel");
            }

            SelectedModel = selectedModel;
            SelectedNodes = selectedModel.WorkflowGraphView.SelectedNodes;
            OnSelectionChanged(EventArgs.Empty);
        }
    }
}
