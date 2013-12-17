using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public abstract class DialogLauncher
    {
        TypeVisualizerDialog visualizerDialog;

        public Rectangle Bounds { get; set; }

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
                visualizerDialog = new TypeVisualizerDialog();
                visualizerDialog.Load += delegate
                {
                    var bounds = Bounds;
                    if (!bounds.IsEmpty && SystemInformation.VirtualScreen.Contains(bounds))
                    {
                        if (bounds.Size.IsEmpty) visualizerDialog.DesktopLocation = bounds.Location;
                        else visualizerDialog.DesktopBounds = bounds;
                    }
                };

                visualizerDialog.FormClosing += delegate { Bounds = visualizerDialog.DesktopBounds; };
                visualizerDialog.FormClosed += delegate
                {
                    visualizerDialog.Dispose();
                    visualizerDialog = null;
                };

                InitializeComponents(visualizerDialog, provider);
                if (owner != null) visualizerDialog.Show(owner);
                else visualizerDialog.Show();
            }

            visualizerDialog.Activate();
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
