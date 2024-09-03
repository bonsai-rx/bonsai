﻿using System;
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
        readonly ImageList imageList;
        readonly ImageList stateImageList;

        public ExplorerTreeView()
        {
            imageList = new();
            stateImageList = new();
            imageList.Images.Add(Resources.WorkflowEditableImage);
            imageList.Images.Add(Resources.WorkflowReadOnlyImage);
#if NETFRAMEWORK
            stateImageList.Images.Add(Resources.StatusReadyImage);
            stateImageList.Images.Add(Resources.StatusBlockedImage);
#else
            // TreeView.StateImageList.ImageSize is internally scaled according to initial system DPI (not font).
            // To avoid excessive scaling of images we must prepare correctly sized ImageList beforehand.
            const float DefaultDpi = 96f;
            using var graphics = CreateGraphics();
            var dpiScale = graphics.DpiY / DefaultDpi;
            stateImageList.ImageSize = new Size(
                (int)(16 * dpiScale),
                (int)(16 * dpiScale));
            stateImageList.Images.Add(ResizeMakeBorder(Resources.StatusReadyImage, stateImageList.ImageSize));
            stateImageList.Images.Add(ResizeMakeBorder(Resources.StatusBlockedImage, stateImageList.ImageSize));

            static Bitmap ResizeMakeBorder(Bitmap original, Size newSize)
            {
                //TODO: DrawImageUnscaledAndClipped gives best results but blending is not great
                var image = new Bitmap(newSize.Width, newSize.Height, original.PixelFormat);
                using var graphics = Graphics.FromImage(image);
                var offsetX = (newSize.Width - original.Width) / 2;
                var offsetY = (newSize.Height - original.Height) / 2;
                graphics.DrawImageUnscaledAndClipped(original, new Rectangle(offsetX, offsetY, original.Width, original.Height));
                return image;
            }
#endif

            StateImageList = stateImageList;
            ImageList = imageList;
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
            AddWorkflow(rootNode.Nodes, null, workflowBuilder.Workflow, ExplorerNodeType.Editable);

            static void AddWorkflow(
                TreeNodeCollection nodes,
                WorkflowEditorPath basePath,
                ExpressionBuilderGraph workflow,
                ExplorerNodeType parentNodeType)
            {
                for (int i = 0; i < workflow.Count; i++)
                {
                    var builder = workflow[i].Value;
                    if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder &&
                        workflowBuilder.Workflow != null)
                    {
                        var nodeType = parentNodeType == ExplorerNodeType.ReadOnly || workflowBuilder is IncludeWorkflowBuilder
                            ? ExplorerNodeType.ReadOnly
                            : ExplorerNodeType.Editable;
                        var displayName = ExpressionBuilder.GetElementDisplayName(builder);
                        var builderPath = new WorkflowEditorPath(i, basePath);
                        var node = nodes.Add(displayName);
                        node.ImageIndex = node.SelectedImageIndex = GetImageIndex(nodeType);
                        node.Tag = builderPath;
                        AddWorkflow(node.Nodes, builderPath, workflowBuilder.Workflow, nodeType);
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

        private static int GetImageIndex(ExplorerNodeType status)
        {
            return status switch
            {
                ExplorerNodeType.Editable => 0,
                ExplorerNodeType.ReadOnly => 1,
                _ => throw new ArgumentException("Invalid node type.", nameof(status))
            };
        }

        private static int GetStateImageIndex(ExplorerNodeStatus status)
        {
            return status switch
            {
                ExplorerNodeStatus.Ready => -1,
                ExplorerNodeStatus.Blocked => 1,
                _ => throw new ArgumentException("Invalid node status.", nameof(status))
            };
        }

        public void SetNodeStatus(ExplorerNodeStatus status)
        {
            var imageIndex = GetStateImageIndex(status);
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
            var imageIndex = GetStateImageIndex(status);
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

    enum ExplorerNodeType
    {
        Editable,
        ReadOnly
    }

    enum ExplorerNodeStatus
    {
        Ready,
        Blocked
    }
}
