using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    abstract class DialogLauncher
    {
        public Rectangle Bounds { get; set; }

        public FormWindowState WindowState { get; set; }

        public bool Visible
        {
            get { return VisualizerDialog != null; }
        }

        protected LauncherDialog VisualizerDialog { get; private set; }

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
            if (VisualizerDialog == null)
            {
                VisualizerDialog = CreateVisualizerDialog(provider);
                VisualizerDialog.Load += delegate
                {
                    var bounds = Bounds;
                    if (!bounds.IsEmpty && (SystemInformation.VirtualScreen.Contains(bounds) || WindowState != FormWindowState.Normal))
                    {
                        VisualizerDialog.LayoutBounds = bounds;
                        VisualizerDialog.WindowState = WindowState;
                    }
                };

                VisualizerDialog.FormClosed += delegate
                {
                    Bounds = VisualizerDialog.LayoutBounds;
                    if (VisualizerDialog.WindowState == FormWindowState.Minimized)
                    {
                        WindowState = FormWindowState.Normal;
                    }
                    else WindowState = VisualizerDialog.WindowState;
                    VisualizerDialog.Dispose();
                };

                VisualizerDialog.HandleDestroyed += (sender, e) => VisualizerDialog = null;
                InitializeComponents(VisualizerDialog, provider);
                if (VisualizerDialog.TopLevel)
                {
                    if (owner != null) VisualizerDialog.Show(owner);
                    else VisualizerDialog.Show();
                }
            }

            VisualizerDialog.Activate();
        }

        protected virtual LauncherDialog CreateVisualizerDialog(IServiceProvider provider)
        {
            return new LauncherDialog();
        }

        protected abstract void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider);

        public virtual void Hide()
        {
            if (VisualizerDialog != null)
            {
                VisualizerDialog.Close();
            }
        }

        protected class LauncherDialog : TypeVisualizerDialog
        {
            SizeF inverseScaleFactor;
            SizeF scaleFactor;

            internal Rectangle LayoutBounds
            {
                get
                {
                    var desktopBounds = Bounds;
                    if (WindowState != FormWindowState.Normal)
                    {
                        desktopBounds.Size = RestoreBounds.Size;
                    }
                    else desktopBounds = DesktopBounds;
                    return ScaleBounds(desktopBounds, inverseScaleFactor);
                }
                set
                {
                    var bounds = ScaleBounds(value, scaleFactor);
                    if (bounds.Size.IsEmpty) DesktopLocation = bounds.Location;
                    else DesktopBounds = bounds;
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
