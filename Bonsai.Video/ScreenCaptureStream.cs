using AForge.Video;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Video
{
    [Description("Produces a sequence of images captured from the desktop screen.")]
    public class ScreenCaptureStream : VideoCapture
    {
        int frameInterval;
        Rect captureRegion;

        [Description("Determines whether to overlay the mouse cursor on captured images.")]
        public bool HideCursor { get; set; }

        [Description("The rectangle region of the screen to capture.")]
        [Editor("Bonsai.Vision.Design.IplImageOutputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Rect CaptureRegion { get; set; }

        [Description("The interval between each screen grab, in milliseconds.")]
        public int FrameInterval
        {
            get { return frameInterval; }
            set
            {
                frameInterval = value;
                var videoSource = (AForge.Video.ScreenCaptureStream)VideoSource;
                if (videoSource != null)
                {
                    videoSource.FrameInterval = frameInterval;
                }
            }
        }

        protected override IVideoSource CreateVideoSource()
        {
            Rectangle region;
            captureRegion = CaptureRegion;
            if (captureRegion.Width == 0 || captureRegion.Height == 0)
            {
                region = Rectangle.Empty;
                foreach (var screen in Screen.AllScreens)
                {
                    region = Rectangle.Union(region, screen.Bounds);
                }
            }
            else region = new Rectangle(captureRegion.X, captureRegion.Y, captureRegion.Width, captureRegion.Height);
            return new AForge.Video.ScreenCaptureStream(region, frameInterval);
        }

        protected override IplImage ProcessFrame(Bitmap bitmap)
        {
            if (!HideCursor)
            {
                NativeMethods.CURSORINFO cursorInfo;
                cursorInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.CURSORINFO));
                if (NativeMethods.GetCursorInfo(out cursorInfo))
                {
                    NativeMethods.ICONINFO iconInfo;
                    if (cursorInfo.flags == NativeMethods.CURSOR_SHOWING &&
                        NativeMethods.GetIconInfo(cursorInfo.hCursor, out iconInfo))
                    {
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            var region = captureRegion;
                            var x = cursorInfo.ptScreenPos.x - region.X - iconInfo.xHotspot;
                            var y = cursorInfo.ptScreenPos.y - region.Y - iconInfo.yHotspot;
                            try
                            {
                                if (iconInfo.hbmColor == IntPtr.Zero)
                                {
                                    using (var mask = Bitmap.FromHbitmap(iconInfo.hbmMask))
                                    using (var cursorBitmap = new Bitmap(mask.Width, mask.Height / 2))
                                    using (var cursorGraphics = Graphics.FromImage(cursorBitmap))
                                    {
                                        var cursorHdc = cursorGraphics.GetHdc();
                                        var maskHdc = NativeMethods.CreateCompatibleDC(cursorHdc);
                                        NativeMethods.SelectObject(maskHdc, iconInfo.hbmMask);
                                        try
                                        {
                                            NativeMethods.BitBlt(cursorHdc, 0, 0, mask.Width, mask.Width, maskHdc, 0, mask.Width, NativeMethods.TernaryRasterOperations.SRCCOPY);
                                            NativeMethods.BitBlt(cursorHdc, 0, 0, mask.Width, mask.Width, maskHdc, 0, 0, NativeMethods.TernaryRasterOperations.SRCINVERT);
                                        }
                                        finally
                                        {
                                            NativeMethods.DeleteDC(maskHdc);
                                            cursorGraphics.ReleaseHdc(cursorHdc);
                                        }

                                        cursorBitmap.MakeTransparent(Color.White);
                                        graphics.DrawImageUnscaled(cursorBitmap, x, y);
                                    }
                                }
                                else
                                {
                                    var hdc = graphics.GetHdc();
                                    try
                                    {
                                        NativeMethods.DrawIconEx(
                                            hdc, x, y, cursorInfo.hCursor,
                                            0, 0, 0, IntPtr.Zero, NativeMethods.DI_NORMAL);
                                    }
                                    finally { graphics.ReleaseHdc(hdc); }
                                }
                            }
                            finally
                            {
                                NativeMethods.DeleteObject(iconInfo.hbmColor);
                                NativeMethods.DeleteObject(iconInfo.hbmMask);
                            }
                        }
                    }
                }
            }

            return base.ProcessFrame(bitmap);
        }
    }
}
