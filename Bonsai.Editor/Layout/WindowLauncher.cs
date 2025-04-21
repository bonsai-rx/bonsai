using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    abstract class WindowLauncher
    {
        public Rectangle Bounds { get; set; }

        public FormWindowState WindowState { get; set; }

        public bool Visible
        {
            get { return VisualizerWindow != null; }
        }

        protected LauncherWindow VisualizerWindow { get; private set; }

        public void Show()
        {
            Show(null);
        }

        public void Show(IServiceProvider provider)
        {
            Show(null, provider);
        }

        public virtual void Show(IWin32Window owner, IServiceProvider provider)
        {
            if (VisualizerWindow == null)
            {
                VisualizerWindow = CreateVisualizerWindow(provider);
                VisualizerWindow.Load += delegate
                {
                    var bounds = Bounds;
                    if (!bounds.IsEmpty && (SystemInformation.VirtualScreen.IntersectsWith(bounds) || WindowState != FormWindowState.Normal))
                    {
                        VisualizerWindow.LayoutBounds = bounds;
                        VisualizerWindow.WindowState = WindowState;
                    }
                };

                VisualizerWindow.FormClosed += delegate
                {
                    Bounds = VisualizerWindow.LayoutBounds;
                    if (VisualizerWindow.WindowState == FormWindowState.Minimized)
                    {
                        WindowState = FormWindowState.Normal;
                    }
                    else WindowState = VisualizerWindow.WindowState;
                    VisualizerWindow.Dispose();
                };

                VisualizerWindow.HandleDestroyed += (sender, e) => VisualizerWindow = null;
                InitializeComponents(VisualizerWindow, provider);
                if (VisualizerWindow.TopLevel)
                {
                    if (owner != null) VisualizerWindow.Show(owner);
                    else VisualizerWindow.Show();
                }
            }

            VisualizerWindow.Activate();
        }

        protected virtual LauncherWindow CreateVisualizerWindow(IServiceProvider provider)
        {
            return new LauncherWindow();
        }

        protected abstract void InitializeComponents(TypeVisualizerWindow visualizerWindow, IServiceProvider provider);

        public virtual void Hide()
        {
            if (VisualizerWindow != null)
            {
                VisualizerWindow.Close();
            }
        }

        protected class LauncherWindow : TypeVisualizerWindow
        {
            SizeF inverseScaleFactor;
            SizeF scaleFactor;

            internal Rectangle LayoutBounds
            {
                get
                {
                    var layoutBounds = WindowState != FormWindowState.Normal ? RestoreBounds : Bounds;
                    return ScaleBounds(layoutBounds, inverseScaleFactor);
                }
                set
                {
                    var layoutBounds = ScaleBounds(value, scaleFactor);
                    if (layoutBounds.Width > 0)
                    {
                        Bounds = layoutBounds;
                    }
                }
            }

            protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
            {
                scaleFactor = factor;
                inverseScaleFactor = new SizeF(1f / factor.Width, 1f / factor.Height);
                base.ScaleControl(factor, specified);
            }

            private static Rectangle ScaleBounds(Rectangle bounds, SizeF scaleFactor)
            {
                bounds.Location = Point.Round(new PointF(bounds.X * scaleFactor.Width, bounds.Y * scaleFactor.Height));
                bounds.Size = Size.Round(new SizeF(bounds.Width * scaleFactor.Width, bounds.Height * scaleFactor.Height));
                return bounds;
            }
        }
    }
}
