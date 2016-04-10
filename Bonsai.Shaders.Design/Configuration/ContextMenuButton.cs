using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    class ContextMenuButton : Button
    {
        static readonly Size ArrowOffset = new Size(14, 1);
        static readonly Size ArrowSize = new Size(7, 4);

        protected override void OnClick(EventArgs e)
        {
            var menu = ContextMenuStrip;
            if (menu == null || !menu.IsHandleCreated)
            {
                base.OnClick(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            var menu = ContextMenuStrip;
            if (menu != null && mevent.Button == MouseButtons.Left)
            {
                var location = Point.Empty;
                location.Y += Height;
                menu.Show(this, location);
            }

            base.OnMouseDown(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            var menu = ContextMenuStrip;
            if (menu != null)
            {
                var rectangle = ClientRectangle;
                var originX = rectangle.Width - ArrowOffset.Width;
                var originY = rectangle.Height / 2 - ArrowOffset.Height;
                var brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;
                var points = new[]
                {
                    new Point(originX, originY),
                    new Point(originX + ArrowSize.Width, originY),
                    new Point(originX + ArrowSize.Width / 2, originY + ArrowSize.Height)
                };
                pevent.Graphics.FillPolygon(brush, points);
            }
        }
    }
}
