using System;
using System.Runtime.InteropServices;

namespace Bonsai.NuGet.Design
{
    static class NativeMethods
    {
        internal static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
