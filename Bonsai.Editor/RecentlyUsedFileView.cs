using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class RecentlyUsedFileView : TreeView
    {
        const int TVS_NOTOOLTIPS = 0x80;
        const int TVS_NOHSCROLL = 0x8000;
        Font recentFileNameFont;
        Font recentFilePathFont;
        int leftMargin;
        int lineHeight;

        public RecentlyUsedFileView()
        {
            ShowLines = false;
            ShowRootLines = false;
            FullRowSelect = true;
            HotTracking = true;
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                recentFilePathFont = value;
                recentFileNameFont = new Font(value.FontFamily, value.SizeInPoints + 1, FontStyle.Bold);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.Style |= TVS_NOTOOLTIPS;
                parameters.Style |= TVS_NOHSCROLL;
                return parameters;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            var itemHeight = 20 * factor.Height;
            leftMargin = (int)(2 * factor.Height);
            lineHeight = (int)itemHeight;
            ItemHeight = (int)(2.5f * itemHeight);
            base.ScaleControl(factor, specified);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var node = e.Node;
            var bounds = e.Bounds;
            bounds.Width = ClientSize.Width;
            var itemMargin = lineHeight / 4;
            var nameBounds = new Rectangle(bounds.X, bounds.Y + itemMargin, bounds.Width, lineHeight);
            var pathBounds = new Rectangle(bounds.X, bounds.Y + itemMargin + lineHeight, bounds.Width, lineHeight);
            var fillBounds = new Rectangle(bounds.X - leftMargin, bounds.Y, bounds.Width + leftMargin, bounds.Height);
            var focusBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - leftMargin, bounds.Height);
            var hot = (e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot;
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            using (var brush = new SolidBrush(hot ? LineColor : BackColor))
            {
                e.Graphics.FillRectangle(brush, fillBounds);
            }
            TextRenderer.DrawText(e.Graphics, node.Name, recentFileNameFont, nameBounds, ForeColor, TextFormatFlags.Left);
            TextRenderer.DrawText(e.Graphics, node.Text, recentFilePathFont, pathBounds, Color.Gray, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            if (selected) ControlPaint.DrawFocusRectangle(e.Graphics, focusBounds, ForeColor, BackColor);
            base.OnDrawNode(e);
        }
    }
}
