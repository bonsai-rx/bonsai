using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    abstract class DialogLauncher
    {
        TypeVisualizerDialog visualizerDialog;

        public Rectangle Bounds { get; set; }

        public FormWindowState WindowState { get; set; }

        public bool Visible
        {
            get { return visualizerDialog != null; }
        }

        protected TypeVisualizerDialog VisualizerDialog
        {
            get { return visualizerDialog; }
        }

        public void Show()
        {
            Show(null);
        }

        public void Show(IServiceProvider provider)
        {
            InitializeComponents(null, provider);
        }

        public virtual void Show(IWin32Window owner, IServiceProvider provider)
        {
            if (visualizerDialog == null)
            {
                visualizerDialog = CreateVisualizerDialog(provider);
                visualizerDialog.Load += delegate
                {
                    var bounds = Bounds;
                    if (!bounds.IsEmpty && (SystemInformation.VirtualScreen.Contains(bounds) || WindowState != FormWindowState.Normal))
                    {
                        if (bounds.Size.IsEmpty) visualizerDialog.DesktopLocation = bounds.Location;
                        else visualizerDialog.DesktopBounds = bounds;
                        visualizerDialog.WindowState = WindowState;
                    }
                };

                visualizerDialog.FormClosed += delegate
                {
                    var desktopBounds = Bounds;
                    if (visualizerDialog.WindowState != FormWindowState.Normal)
                    {
                        desktopBounds.Size = visualizerDialog.RestoreBounds.Size;
                    }
                    else desktopBounds = visualizerDialog.DesktopBounds;

                    Bounds = desktopBounds;
                    if (visualizerDialog.WindowState == FormWindowState.Minimized)
                    {
                        WindowState = FormWindowState.Normal;
                    }
                    else WindowState = visualizerDialog.WindowState;
                    visualizerDialog.Dispose();
                };

                visualizerDialog.HandleDestroyed += (sender, e) => visualizerDialog = null;
                InitializeComponents(visualizerDialog, provider);
                if (visualizerDialog.TopLevel)
                {
                    if (owner != null) visualizerDialog.Show(owner);
                    else visualizerDialog.Show();
                }
            }

            visualizerDialog.Activate();
        }

        protected virtual TypeVisualizerDialog CreateVisualizerDialog(IServiceProvider provider)
        {
            return new TypeVisualizerDialog();
        }

        protected abstract void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider);

        public virtual void Hide()
        {
            if (visualizerDialog != null)
            {
                visualizerDialog.Close();
            }
        }
    }
}
