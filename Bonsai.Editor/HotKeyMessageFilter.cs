using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class HotKeyMessageFilter : IMessageFilter
    {
        public bool TabState { get; set; }

        public bool SuppressSystemKeyPress { get; set; }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYUP = 0x101;
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            switch (m.Msg)
            {
                case WM_KEYDOWN:
                    if ((Keys)m.WParam == Keys.Tab) TabState = true;
                    break;
                case WM_KEYUP:
                    if ((Keys)m.WParam == Keys.Tab) TabState = false;
                    break;
                case WM_SYSKEYDOWN:
                    if (SuppressSystemKeyPress) return true;
                    break;
            }

            return false;
        }
    }
}
