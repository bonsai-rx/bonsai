﻿using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ToolStripExtendedRenderer : ToolStripProfessionalRenderer
    {
        public ToolStripExtendedRenderer()
            : this(new ExtendedColorTable())
        {
        }

        public ToolStripExtendedRenderer(ExtendedColorTable extendedColorTable)
            : base(extendedColorTable)
        {
            ColorTable = extendedColorTable;
            RoundedEdges = false;
        }

        public new ExtendedColorTable ColorTable { get; private set; }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            var rectangle = e.ArrowRectangle;
            var center = new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);
            var offset = rectangle.Width / 4;

            Point[] arrow = null;
            switch (e.Direction)
            {
                case ArrowDirection.Down:
                    arrow = new Point[] {
                        new Point(center.X - offset, center.Y - 1), 
                        new Point(center.X + offset + 1, center.Y - 1), 
                        new Point(center.X, center.Y + offset)
                    };
                    break;
                case ArrowDirection.Left:
                    arrow = new Point[] {
                        new Point(center.X + offset, center.Y - offset - 1), 
                        new Point(center.X + offset, center.Y + offset + 1), 
                        new Point(center.X - 1, center.Y)
                    };
                    break;
                case ArrowDirection.Right:
                    arrow = new Point[] {
                        new Point(center.X - offset, center.Y - offset - 1), 
                        new Point(center.X - offset, center.Y + offset + 1), 
                        new Point(center.X + 1, center.Y)
                    };
                    break;
                case ArrowDirection.Up:
                    arrow = new Point[] {
                        new Point(center.X - offset, center.Y + 1), 
                        new Point(center.X + offset + 1, center.Y + 1), 
                        new Point(center.X, center.Y - offset)
                    };
                    break;
            }

            var arrowColor = e.Item.Enabled ? e.ArrowColor : ColorTable.ControlDark;
            using (var brush = new SolidBrush(arrowColor))
            {
                e.Graphics.FillPolygon(brush, arrow);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is StatusStrip) return;
            base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item is ToolStripStatusLabel statusLabel && statusLabel.Spring)
            {
                TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont,
                    new Rectangle(Point.Empty, e.TextRectangle.Size),
                    e.TextColor, Color.Transparent,
                    TextFormatFlags.Default);
            }
            else base.OnRenderItemText(e);
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            if (item is ToolStripDropDownItem dropDown)
            {
                foreach (ToolStripItem dropItem in dropDown.DropDownItems)
                {
                    InitializeItem(dropItem);
                }
            }

            base.InitializeItem(item);
        }
    }
}
