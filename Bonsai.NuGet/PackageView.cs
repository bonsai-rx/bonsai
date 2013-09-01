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
        const string InstallButtonText = "Install";
        static readonly Rectangle InstallButtonBounds = new Rectangle(10, 2, 75, 23);
        const int WM_NCMOUSEHOVER = 0x02a0;
        const int WM_MOUSEHOVER = 0x02a1;
        const int WM_NCMOUSELEAVE = 0x02a2;
        const int WM_MOUSELEAVE = 0x02a3;
        const int WM_NOTIFY = 0x004e;
        const int TVS_NOHSCROLL = 0x8000;

        public PackageView()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        [Category("Action")]
        public event TreeViewEventHandler InstallClick;

        private void OnInstallClick(TreeViewEventArgs e)
        {
            var handler = InstallClick;
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
            if (InstallHitTest(e.Location) && e.Button == MouseButtons.Left)
            {
                OnInstallClick(new TreeViewEventArgs(SelectedNode, TreeViewAction.ByMouse));
            }
            base.OnMouseClick(e);
        }

        private bool InstallHitTest(Point pt)
        {
            var hitTestInfo = HitTest(pt);
            if (hitTestInfo.Node != null && hitTestInfo.Node == SelectedNode)
            {
                var buttonBounds = GetInstallButtonBounds(hitTestInfo.Node.Bounds);
                return buttonBounds.Contains(pt);
            }

            return false;
        }

        private int RightMargin
        {
            get { return Width - SystemInformation.VerticalScrollBarWidth; }
        }

        private Rectangle GetInstallButtonBounds(Rectangle nodeBounds)
        {
            nodeBounds.X = RightMargin - InstallButtonBounds.Width - InstallButtonBounds.X;
            nodeBounds.Y += InstallButtonBounds.Y;
            nodeBounds.Size = InstallButtonBounds.Size;
            return nodeBounds;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var color = (e.State & TreeNodeStates.Selected) != 0 ? SystemColors.HighlightText : SystemColors.WindowText;
            var bounds = e.Bounds;
            bounds.Width = RightMargin - bounds.X;
            var bold = new Font(Font, FontStyle.Bold);

            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                var font = Font;
                var buttonBounds = GetInstallButtonBounds(bounds);
                bounds.Width -= buttonBounds.Width;

                if (VisualStyleRenderer.IsSupported)
                {
                    ButtonRenderer.DrawButton(e.Graphics, buttonBounds, InstallButtonText, font, false, PushButtonState.Normal);
                }
                else
                {
                    var buttonTextSize = TextRenderer.MeasureText(InstallButtonText, font);
                    var buttonTextOffset = new Point(
                        buttonBounds.Location.X + (buttonBounds.Size.Width - buttonTextSize.Width) / 2,
                        buttonBounds.Location.Y + (buttonBounds.Size.Height - buttonTextSize.Height) / 2);
                    ControlPaint.DrawButton(e.Graphics, buttonBounds, ButtonState.Normal);
                    TextRenderer.DrawText(e.Graphics, InstallButtonText, font, buttonTextOffset, SystemColors.ControlText);
                }
            }

            var lines = e.Node.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            TextRenderer.DrawText(e.Graphics, lines[0], bold, bounds, color, TextFormatFlags.WordBreak);

            if (lines.Length > 1)
            {
                bounds.Y += TextRenderer.MeasureText(lines[0], bold, bounds.Size, TextFormatFlags.WordBreak).Height;
                TextRenderer.DrawText(e.Graphics, lines[1], Font, bounds, color, TextFormatFlags.WordBreak);
            }

            base.OnDrawNode(e);
        }
    }
}
