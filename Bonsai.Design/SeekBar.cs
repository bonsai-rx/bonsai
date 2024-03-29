﻿using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a horizontal seek bar.
    /// </summary>
    public class SeekBar : HScrollBar
    {
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        /// <inheritdoc/>
        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
        }
    }
}
