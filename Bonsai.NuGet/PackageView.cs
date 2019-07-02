using Bonsai.NuGet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Bonsai.NuGet
{
    class PackageView : TreeView
    {
        const int WM_NCMOUSEHOVER = 0x02a0;
        const int WM_MOUSEHOVER = 0x02a1;
        const int WM_NCMOUSELEAVE = 0x02a2;
        const int WM_MOUSELEAVE = 0x02a3;
        const int WM_NOTIFY = 0x004e;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONDBLCLK = 0x0203;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONDBLCLK = 0x0206;
        const int TVS_NOHSCROLL = 0x8000;
        const int TVM_SETEXTENDEDSTYLE = 0x112C;
        const int TVS_EX_DOUBLEBUFFER = 0x0004;

        Font boldFont;
        int boundsMargin;
        int verticalScrollBarWidth;
        Rectangle operationButtonBounds;
        readonly Image packageViewNodeCheckedImage;
        static readonly Rectangle DefaultOperationButtonBounds = new Rectangle(10, 2, 75, 23);
        const int DefaultBoundsMargin = 5;

        public PackageView()
        {
            ShowLines = false;
            FullRowSelect = true;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            packageViewNodeCheckedImage = Resources.PackageViewNodeCheckedImage;
            boundsMargin = DefaultBoundsMargin;
            verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            operationButtonBounds = DefaultOperationButtonBounds;
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

        public string OperationText { get; set; }

        public bool CanSelectNodes { get; set; }

        private void OnOperationClick(TreeViewEventArgs e)
        {
            var handler = OperationClick;
            if (handler != null)
            {
                handler(this, e);
            }
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

        static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            // slightly dampened scale factor for right margin
            var widthScaleFactor = factor.Height * 0.5f + 0.5f;
            verticalScrollBarWidth = (int)(SystemInformation.VerticalScrollBarWidth * widthScaleFactor);
            boundsMargin = (int)(DefaultBoundsMargin * factor.Height);
            operationButtonBounds = new Rectangle(
                (int)(DefaultOperationButtonBounds.X * factor.Height),
                (int)(DefaultOperationButtonBounds.Y * factor.Height),
                (int)(DefaultOperationButtonBounds.Width * factor.Height),
                (int)(DefaultOperationButtonBounds.Height * factor.Height));
            base.ScaleControl(factor, specified);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!IsRunningOnMono())
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
                case WM_MOUSELEAVE:
                case WM_NCMOUSELEAVE:
                case WM_MOUSEHOVER:
                case WM_NCMOUSEHOVER:
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

        private Rectangle GetOperationButtonBounds(Rectangle nodeBounds)
        {
            nodeBounds.X = RightMargin - operationButtonBounds.Width - operationButtonBounds.X;
            nodeBounds.Y += operationButtonBounds.Y;
            nodeBounds.Size = operationButtonBounds.Size;
            return nodeBounds;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var color = (e.State & TreeNodeStates.Selected) != 0 ? SystemColors.HighlightText : SystemColors.WindowText;
            var bounds = e.Bounds;
            bounds.Width = RightMargin - bounds.X;

            if (e.Node.Tag == null)
            {
                TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, bounds, color,
                                      TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter);
            }
            else
            {
                if (e.Node.Checked)
                {
                    var checkedImageX = RightMargin - packageViewNodeCheckedImage.Width - boundsMargin;
                    var checkedImageY = bounds.Y + operationButtonBounds.Y;
                    e.Graphics.DrawImage(packageViewNodeCheckedImage, checkedImageX, checkedImageY);
                    bounds.Width -= packageViewNodeCheckedImage.Width;
                }
                else if ((e.State & TreeNodeStates.Selected) != 0)
                {
                    var font = Font;
                    var buttonBounds = GetOperationButtonBounds(bounds);
                    bounds.Width -= buttonBounds.Width + boundsMargin * 2;

                    if (VisualStyleRenderer.IsSupported)
                    {
                        ButtonRenderer.DrawButton(e.Graphics, buttonBounds, OperationText, font, false, PushButtonState.Normal);
                    }
                    else
                    {
                        var buttonTextSize = TextRenderer.MeasureText(OperationText, font);
                        var buttonTextOffset = new Point(
                            buttonBounds.Location.X + (buttonBounds.Size.Width - buttonTextSize.Width) / 2,
                            buttonBounds.Location.Y + (buttonBounds.Size.Height - buttonTextSize.Height) / 2);
                        ControlPaint.DrawButton(e.Graphics, buttonBounds, ButtonState.Normal);
                        TextRenderer.DrawText(e.Graphics, OperationText, font, buttonTextOffset, SystemColors.ControlText);
                    }
                }

                var lines = e.Node.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                TextRenderer.DrawText(e.Graphics, lines[0], boldFont, bounds, color, TextFormatFlags.WordEllipsis);

                if (lines.Length > 1)
                {
                    bounds.Y += TextRenderer.MeasureText(lines[0], boldFont, bounds.Size, TextFormatFlags.WordEllipsis).Height;
                    TextRenderer.DrawText(e.Graphics, lines[1], Font, bounds, color, TextFormatFlags.WordBreak);
                }
            }

            base.OnDrawNode(e);
        }
    }
}
