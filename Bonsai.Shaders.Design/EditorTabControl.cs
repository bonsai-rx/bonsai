using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Design
{
    class EditorTabControl : TabControl
    {
        const int BorderSize = 1;
        const int TCM_ADJUSTRECT = 0x1328;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == TCM_ADJUSTRECT)
            {
                var rect = (RECT)m.GetLParam(typeof(RECT));
                rect.Left = Left - Margin.Left - BorderSize;
                rect.Right = Right + Margin.Right + BorderSize;
                rect.Top = Top - Margin.Top - BorderSize;
                rect.Bottom = Bottom + Margin.Bottom + BorderSize;
                Marshal.StructureToPtr(rect, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
