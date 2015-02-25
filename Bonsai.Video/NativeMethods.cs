using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Video
{
    static class NativeMethods
    {
        internal const int CURSOR_SHOWING = 0x00000001;
        internal const int CURSOR_SUPPRESSED = 0x00000002;
        internal const int DI_NORMAL = 0x0003;

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            internal int x;
            internal int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CURSORINFO
        {
            internal int cbSize;
            internal int flags;
            internal IntPtr hCursor;
            internal POINT ptScreenPos;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        internal enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        [DllImport("user32.dll")]
        internal static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        internal static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DrawIconEx(
            IntPtr hdc,
            int xLeft,
            int yTop,
            IntPtr hIcon,
            int cxWidth,
            int cyHeight,
            int istepIfAniCur,
            IntPtr hbrFlickerFreeDraw,
            int diFlags);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern bool BitBlt(
            IntPtr hdc,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            TernaryRasterOperations dwRop);
    }
}
