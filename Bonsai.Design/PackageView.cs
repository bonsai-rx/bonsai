using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    class PackageView : TreeView
    {
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

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var color = (e.State & TreeNodeStates.Focused) != 0 ? SystemColors.HighlightText : SystemColors.WindowText;
            var bounds = e.Bounds;
            bounds.Width = Width - bounds.X;
            var bold = new Font(Font, FontStyle.Bold);

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
