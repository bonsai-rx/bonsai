using System;
using System.Runtime.InteropServices;

namespace Bonsai.NuGet
{
    static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
