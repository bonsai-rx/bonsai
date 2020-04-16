using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class ToolboxTreeView : TreeView
    {
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
    }
}
