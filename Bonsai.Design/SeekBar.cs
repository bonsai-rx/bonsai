using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class SeekBar : HScrollBar
    {
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                    var lParam = m.LParam.ToInt64();
                    var x = (int)(lParam & ushort.MaxValue);
                    if (x >= SystemInformation.HorizontalScrollBarArrowWidth && x < Width - SystemInformation.HorizontalScrollBarArrowWidth)
                    {
                        x -= SystemInformation.HorizontalScrollBarArrowWidth;
                        var barWidth = Width - 2 * SystemInformation.HorizontalScrollBarArrowWidth;
                        Value = (int)(((float)x / barWidth) * (Maximum - Minimum));
                    }
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
        }
    }
}
