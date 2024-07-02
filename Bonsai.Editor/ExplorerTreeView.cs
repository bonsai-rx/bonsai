using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Properties;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    class ExplorerTreeView : ToolboxTreeView
    {
        bool activeDoubleClick;
        readonly ImageList iconList;

        public ExplorerTreeView()
        {
            iconList = new()
            {
                ColorDepth = ColorDepth.Depth8Bit,
                ImageSize = new Size(16, 16),
                TransparentColor = Color.Transparent
            };
            StateImageList = iconList;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            iconList.Images.Clear();
            iconList.Images.Add(Resources.StatusReadyImage);
            iconList.Images.Add(Resources.StatusBlockedImage);
            base.ScaleControl(factor, specified);
        }

        protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            if (activeDoubleClick && e.Action == TreeViewAction.Collapse)
                e.Cancel = true;
            activeDoubleClick = false;
            base.OnBeforeCollapse(e);
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            if (activeDoubleClick && e.Action == TreeViewAction.Expand)
                e.Cancel = true;
            activeDoubleClick = false;
            base.OnBeforeExpand(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            activeDoubleClick = e.Clicks > 1;
            base.OnMouseDown(e);
        }

        public void UpdateWorkflow(string name, WorkflowBuilder workflowBuilder)
        {
            BeginUpdate();
            Nodes.Clear();

            var rootNode = Nodes.Add(name);
            AddWorkflow(rootNode.Nodes, null, workflowBuilder.Workflow);

            static void AddWorkflow(TreeNodeCollection nodes, WorkflowEditorPath basePath, ExpressionBuilderGraph workflow)
            {
                for (int i = 0; i < workflow.Count; i++)
                {
                    var builder = workflow[i].Value;
                    if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder &&
                        workflowBuilder.Workflow != null)
                    {
                        var displayName = ExpressionBuilder.GetElementDisplayName(builder);
                        var builderPath = new WorkflowEditorPath(i, basePath);
                        var node = nodes.Add(displayName);
                        node.Tag = builderPath;
                        AddWorkflow(node.Nodes, builderPath, workflowBuilder.Workflow);
                    }
                }
            }

            SetNodeStatus(ExplorerNodeStatus.Ready);
            rootNode.Expand();
            EndUpdate();
        }

        public void SelectNode(WorkflowEditorPath path)
        {
            SelectNode(Nodes, path);
        }

        bool SelectNode(TreeNodeCollection nodes, WorkflowEditorPath path)
        {
            foreach (TreeNode node in nodes)
            {
                var nodePath = (WorkflowEditorPath)node.Tag;
                if (nodePath == path)
                {
                    SelectedNode = node;
                    return true;
                }

                var selected = SelectNode(node.Nodes, path);
                if (selected) break;
            }

            return false;
        }

        private static int GetImageIndex(ExplorerNodeStatus status)
        {
            return status switch
            {
                ExplorerNodeStatus.Ready => 0,
                ExplorerNodeStatus.Blocked => 1,
                _ => throw new ArgumentException("Invalid node status.", nameof(status))
            };
        }

        public void SetNodeStatus(ExplorerNodeStatus status)
        {
            var imageIndex = GetImageIndex(status);
            SetNodeImageIndex(Nodes, imageIndex);

            static void SetNodeImageIndex(TreeNodeCollection nodes, int index)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.StateImageIndex == index)
                        continue;

                    node.StateImageIndex = index;
                    SetNodeImageIndex(node.Nodes, index);
                }
            }
        }

        public void SetNodeStatus(IEnumerable<WorkflowEditorPath> pathElements, ExplorerNodeStatus status)
        {
            var nodes = Nodes;
            var imageIndex = GetImageIndex(status);
            foreach (var path in pathElements.Prepend(null))
            {
                var found = false;
                for (int n = 0; n < nodes.Count; n++)
                {
                    var groupNode = nodes[n];
                    if ((WorkflowEditorPath)groupNode.Tag == path)
                    {
                        groupNode.StateImageIndex = imageIndex;
                        nodes = groupNode.Nodes;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break;
            }
        }
    }

    enum ExplorerNodeStatus
    {
        Ready,
        Blocked
    }
}
