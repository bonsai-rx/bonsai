using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    static class NativeMethods
    {
        internal static int MOD_ALT = 0x1;
        internal static int MOD_CONTROL = 0x2;
        internal static int MOD_SHIFT = 0x4;
        internal static int MOD_WIN = 0x8;
        internal static int WM_HOTKEY = 0x312;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
