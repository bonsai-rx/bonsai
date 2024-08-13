﻿using Bonsai.NuGet.Design.Properties;
using NuGet.Protocol.Core.Types;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    class PackageView : TreeView
    {
        const int WM_NOTIFY = 0x004e;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONDBLCLK = 0x0203;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONDBLCLK = 0x0206;
        const int TVS_NOHSCROLL = 0x8000;
        const int TVM_SETEXTENDEDSTYLE = 0x112C;
        const int TVS_EX_DOUBLEBUFFER = 0x0004;

        Font boldFont;
        SolidBrush nodeHighlight;
        int boundsMargin;
        SizeF buttonSize;
        int verticalScrollBarWidth;
        PackageOperationType operation;
        TreeNode operationHoverNode;
        bool operationButtonState;
        readonly Image packageCheckedImage;
        readonly Image packageWarningImage;
        const int DefaultBoundsMargin = 6;

        public PackageView()
        {
            ShowLines = false;
            HotTracking = true;
            FullRowSelect = true;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            packageCheckedImage = Resources.PackageViewNodeCheckedImage;
            packageWarningImage = Resources.WarningImage;
            boundsMargin = DefaultBoundsMargin;
            verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            nodeHighlight = new SolidBrush(ControlPaint.LightLight(SystemColors.Highlight));
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                boldFont = new Font(value, FontStyle.Bold);
            }
        }

        [Category("Action")]
        public event TreeViewEventHandler OperationClick;

        private Image OperationImage { get; set; }

        public PackageOperationType Operation
        {
            get => operation;
            set
            {
                operation = value;
                OperationImage = operation switch
                {
                    PackageOperationType.Open => Resources.OpenImage,
                    PackageOperationType.Update => Resources.UpdateImage,
                    PackageOperationType.Uninstall => Resources.RemoveImage,
                    PackageOperationType.Install or _ => Resources.DownloadImage
                };
            }
        }

        public bool CanSelectNodes { get; set; }

        private void OnOperationClick(TreeViewEventArgs e)
        {
            OperationClick?.Invoke(this, e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.Style |= TVS_NOHSCROLL;
                return parameters;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            // slightly dampened scale factor for right margin
            var widthScaleFactor = factor.Height * 0.5f + 0.5f;
            verticalScrollBarWidth = (int)(SystemInformation.VerticalScrollBarWidth * widthScaleFactor);
            boundsMargin = (int)(DefaultBoundsMargin * factor.Height);
            using var graphics = CreateGraphics();
            buttonSize = graphics.GetImageSize(OperationImage);
            base.ScaleControl(factor, specified);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!NativeMethods.IsRunningOnMono)
            {
                NativeMethods.SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            }
            boldFont = new Font(Font, FontStyle.Bold);
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NOTIFY:
                    return;
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDBLCLK:
                    if (!CanSelectNodes) return;
                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (CanSelectNodes)
            {
                var node = GetNodeAt(e.Location);
                if (node != null)
                {
                    SelectedNode = node;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (operationHoverNode != node)
                operationButtonState = false;

            if (node != null)
            {
                var buttonBounds = GetOperationButtonBounds(node.Bounds);
                var hoverState = buttonBounds.Contains(e.Location);
                if (operationButtonState != hoverState)
                {
                    var nodeBounds = node.Bounds;
                    nodeBounds.Width = Width - nodeBounds.X;
                    Invalidate(nodeBounds);
                }

                operationButtonState = hoverState;
            }

            operationHoverNode = node;
            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (OperationHitTest(e.Location) && e.Button == MouseButtons.Left)
            {
                OnOperationClick(new TreeViewEventArgs(SelectedNode, TreeViewAction.ByMouse));
            }
            base.OnMouseClick(e);
        }

        private bool OperationHitTest(Point pt)
        {
            var hitTestInfo = HitTest(pt);
            if (hitTestInfo.Node != null && !hitTestInfo.Node.Checked && hitTestInfo.Node == SelectedNode)
            {
                if (hitTestInfo.Node.Tag == null) return false;
                var buttonBounds = GetOperationButtonBounds(hitTestInfo.Node.Bounds);
                return buttonBounds.Contains(pt);
            }

            return false;
        }

        private int RightMargin
        {
            get { return Width - verticalScrollBarWidth; }
        }

        private RectangleF GetOperationButtonBounds(Rectangle nodeBounds)
        {
            // Node bounds span the full text area but we need to account for scroll bar
            return new(
                x: RightMargin - buttonSize.Width - boundsMargin,
                y: nodeBounds.Y + boundsMargin,
                width: buttonSize.Width,
                height: buttonSize.Height);
        }

        private void FillImageBounds(Graphics graphics, Brush brush, Image image, ref Rectangle bounds)
        {
            var imageSize = graphics.GetImageSize(image);
            RectangleF imageBounds = new(
                x: bounds.Right - imageSize.Width,
                y: bounds.Y + boundsMargin,
                width: imageSize.Width,
                height: imageSize.Height);
            graphics.FillRectangle(brush, imageBounds);
        }

        private void DrawPackageImage(Graphics graphics, Image image, ref Rectangle bounds)
        {
            var imageWidth = image.Width * graphics.DpiX / image.HorizontalResolution;
            var imageX = bounds.Right - imageWidth;
            var imageY = bounds.Y + boundsMargin;
            graphics.DrawImage(image, imageX, imageY);
            bounds.Width -= image.Width + boundsMargin;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = false;
            var bounds = e.Bounds;
            bounds.Width = RightMargin - bounds.X;
            var color = (e.State & TreeNodeStates.Selected) != 0
                ? SystemColors.HighlightText
                : SystemColors.WindowText;
            if (e.Node.Tag == null)
            {
                TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, bounds, color,
                                      TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter);
            }
            else
            {
                if (NativeMethods.IsRunningOnMono && (e.State & TreeNodeStates.Selected) != 0)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
                }

                // Draw operation image
                bounds.Width -= boundsMargin;
                if (e.Node.Checked)
                    DrawPackageImage(e.Graphics, packageCheckedImage, ref bounds);
                else if (OperationImage != null)
                {
                    var mousePosition = PointToClient(MousePosition);
                    var buttonBounds = GetOperationButtonBounds(e.Node.Bounds);
                    if (buttonBounds.Contains(mousePosition))
                    {
                        FillImageBounds(
                            e.Graphics,
                            SystemBrushes.ControlLight,
                            OperationImage,
                            ref bounds);
                    }
                    DrawPackageImage(e.Graphics, OperationImage, ref bounds);
                }
                else
                    bounds.Width -= OperationImage.Width + boundsMargin;

                // Draw package version
                var packageVersion = ((IPackageSearchMetadata)e.Node.Tag).Identity.Version.ToString();
                var textSize = TextRenderer.MeasureText(e.Graphics, packageVersion, Font);
                var textPosition = new Point(bounds.Right - textSize.Width - boundsMargin, bounds.Y + boundsMargin);
                TextRenderer.DrawText(e.Graphics, packageVersion, Font, textPosition, color);
                bounds.Width -= textSize.Width + boundsMargin;

                // Draw package warnings
                if (e.Node.Nodes.Count > 0 && e.Node.Nodes[Resources.PackageWarningKey] != null)
                    DrawPackageImage(e.Graphics, packageWarningImage, ref bounds);

                // Add spacing between text boxes
                bounds.Y += boundsMargin;
                bounds.Height -= boundsMargin;

                var lines = e.Node.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                TextRenderer.DrawText(e.Graphics, lines[0], boldFont, bounds, color, TextFormatFlags.WordEllipsis);

                if (lines.Length > 1)
                {
                    var titleSize = TextRenderer.MeasureText(lines[0], boldFont, bounds.Size, TextFormatFlags.WordEllipsis);
                    bounds.Y += titleSize.Height;
                    bounds.Height -= titleSize.Height;
                    TextRenderer.DrawText(e.Graphics, lines[1], Font, bounds, color,
                        TextFormatFlags.TextBoxControl
                        | TextFormatFlags.WordBreak
                        | TextFormatFlags.EndEllipsis);
                }
            }

            base.OnDrawNode(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && nodeHighlight != null)
            {
                nodeHighlight.Dispose();
                nodeHighlight = null;
            }

            base.Dispose(disposing);
        }
    }
}
