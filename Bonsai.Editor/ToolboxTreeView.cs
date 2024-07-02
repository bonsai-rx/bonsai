using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Editor.Themes;

namespace Bonsai.Editor
{
    class ToolboxTreeView : TreeView
    {
        private ToolStripExtendedRenderer renderer;

        public ToolStripExtendedRenderer Renderer
        {
            get => renderer;
            set
            {
                renderer = value;
                UpdateTreeViewSelection(Focused);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            foreach (TreeNode node in Nodes)
            {
                SetNodeEnabled(node);
            }
        }

        void SetNodeEnabled(TreeNode node)
        {
            node.BackColor = Enabled ? Color.Empty : BackColor;
            foreach (TreeNode child in node.Nodes)
            {
                SetNodeEnabled(child);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            UpdateTreeViewSelection(true);
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            UpdateTreeViewSelection(false);
            base.OnLeave(e);
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            UpdateTreeViewSelection(Focused);
            base.OnAfterSelect(e);
        }

        void UpdateTreeViewSelection(bool focused)
        {
            if (Renderer == null)
                return;

            var colorTable = Renderer.ColorTable;
            BackColor = colorTable.ContentPanelBackColor;
            ForeColor = colorTable.WindowText;

            var selectedNode = SelectedNode;
            if (Tag != selectedNode)
            {
                if (Tag is TreeNode previousNode) previousNode.BackColor = Color.Empty;
                Tag = selectedNode;
            }

            if (selectedNode == null) return;
            selectedNode.BackColor = focused ? Color.Empty : colorTable.InactiveCaption;
        }
    }
}
