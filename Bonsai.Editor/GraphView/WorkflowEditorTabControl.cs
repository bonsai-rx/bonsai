using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.GraphView
{
    class WorkflowEditorTabControl : TabControl
    {
        const int TCM_ADJUSTRECT = 0x1328;
        Padding adjustRectangle;

        public Padding AdjustRectangle
        {
            get { return adjustRectangle; }
            set
            {
                adjustRectangle = value;
                OnStyleChanged(EventArgs.Empty);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == TCM_ADJUSTRECT)
            {
                var rect = (RECT)m.GetLParam(typeof(RECT));
                rect.Left = Left - adjustRectangle.Left;
                rect.Right = Right + adjustRectangle.Right;
                rect.Top = Top - adjustRectangle.Top;
                rect.Bottom = Bottom + adjustRectangle.Bottom;
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
