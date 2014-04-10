using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class HotKeyManager
    {
        Form form;

        internal HotKeyManager(Form form)
        {
            this.form = form;
        }

        internal bool RegisterHotKey(int id, Keys key)
        {
            int modifiers = 0;
            if ((key & Keys.Alt) == Keys.Alt)
            {
                modifiers = modifiers | NativeMethods.MOD_ALT;
            }

            if ((key & Keys.Control) == Keys.Control)
            {
                modifiers = modifiers | NativeMethods.MOD_CONTROL;
            }

            if ((key & Keys.Shift) == Keys.Shift)
            {
                modifiers = modifiers | NativeMethods.MOD_SHIFT;
            }

            Keys vk = key & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;
            return NativeMethods.RegisterHotKey((IntPtr)form.Handle, id, (uint)modifiers, (uint)vk);
        }

        internal bool UnregisterHotKey(int id)
        {
            return NativeMethods.UnregisterHotKey(form.Handle, id);
        }
    }
}
