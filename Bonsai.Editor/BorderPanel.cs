using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class BorderPanel : Panel
    {
        public BorderPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BorderColor = Color.Black;
            Padding = new Padding(1);
        }

        [Category("Appearance")]
        public Color BorderColor { get; set; }

        [Browsable(false)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, BorderColor, ButtonBorderStyle.Solid);
        }
    }
}
