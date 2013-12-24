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
        readonly Image packageViewNodeCheckedImage;
        static readonly Rectangle OperationButtonBounds = new Rectangle(10, 2, 75, 23);
        const int BoundsMargin = 5;
        const int WM_NCMOUSEHOVER = 0x02a0;
        const int WM_MOUSEHOVER = 0x02a1;
        const int WM_NCMOUSELEAVE = 0x02a2;
        const int WM_MOUSELEAVE = 0x02a3;
        const int WM_NOTIFY = 0x004e;
        const int TVS_NOHSCROLL = 0x8000;
        const int TVM_SETEXTENDEDSTYLE = 0x112C;
        const int TVS_EX_DOUBLEBUFFER = 0x0004;

        public PackageView()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            packageViewNodeCheckedImage = Resources.PackageViewNodeCheckedImage;
        }

        [Category("Action")]
        public event TreeViewEventHandler OperationClick;

        public string OperationText { get; set; }

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

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!IsRunningOnMono())
            {
                NativeMethods.SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            }
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
            }

            base.WndProc(ref m);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
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
            if (hitTestInfo.Node != null && hitTestInfo.Node == SelectedNode)
            {
                var buttonBounds = GetOperationButtonBounds(hitTestInfo.Node.Bounds);
                return buttonBounds.Contains(pt);
            }

            return false;
        }

        private int RightMargin
        {
            get { return Width - SystemInformation.VerticalScrollBarWidth; }
        }

        private Rectangle GetOperationButtonBounds(Rectangle nodeBounds)
        {
            nodeBounds.X = RightMargin - OperationButtonBounds.Width - OperationButtonBounds.X;
            nodeBounds.Y += OperationButtonBounds.Y;
            nodeBounds.Size = OperationButtonBounds.Size;
            return nodeBounds;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var color = (e.State & TreeNodeStates.Selected) != 0 ? SystemColors.HighlightText : SystemColors.WindowText;
            var bounds = e.Bounds;
            bounds.Width = RightMargin - bounds.X;
            var bold = new Font(Font, FontStyle.Bold);

            if (e.Node.Checked)
            {
                var checkedImageX = RightMargin - packageViewNodeCheckedImage.Width - BoundsMargin;
                var checkedImageY = bounds.Y + OperationButtonBounds.Y;
                e.Graphics.DrawImage(packageViewNodeCheckedImage, checkedImageX, checkedImageY);
                bounds.Width -= packageViewNodeCheckedImage.Width;
            }
            else if ((e.State & TreeNodeStates.Selected) != 0)
            {
                var font = Font;
                var buttonBounds = GetOperationButtonBounds(bounds);
                bounds.Width -= buttonBounds.Width + BoundsMargin * 2;

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
            TextRenderer.DrawText(e.Graphics, lines[0], bold, bounds, color, TextFormatFlags.WordEllipsis);

            if (lines.Length > 1)
            {
                bounds.Y += TextRenderer.MeasureText(lines[0], bold, bounds.Size, TextFormatFlags.WordEllipsis).Height;
                TextRenderer.DrawText(e.Graphics, lines[1], Font, bounds, color, TextFormatFlags.WordBreak);
            }

            base.OnDrawNode(e);
        }
    }
}
