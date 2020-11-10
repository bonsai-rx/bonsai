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

        protected TypeVisualizerDialog VisualizerDialog { get; private set; }

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
                        if (bounds.Size.IsEmpty) VisualizerDialog.DesktopLocation = bounds.Location;
                        else VisualizerDialog.DesktopBounds = bounds;
                        VisualizerDialog.WindowState = WindowState;
                    }
                };

                VisualizerDialog.FormClosed += delegate
                {
                    var desktopBounds = Bounds;
                    if (VisualizerDialog.WindowState != FormWindowState.Normal)
                    {
                        desktopBounds.Size = VisualizerDialog.RestoreBounds.Size;
                    }
                    else desktopBounds = VisualizerDialog.DesktopBounds;

                    Bounds = desktopBounds;
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

        protected virtual TypeVisualizerDialog CreateVisualizerDialog(IServiceProvider provider)
        {
            return new TypeVisualizerDialog();
        }

        protected abstract void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider);

        public virtual void Hide()
        {
            if (VisualizerDialog != null)
            {
                VisualizerDialog.Close();
            }
        }
    }
}
