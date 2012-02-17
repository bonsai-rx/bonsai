using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Drawing;

namespace Bonsai.Design
{
    public class VisualizerDialogLauncher
    {
        InspectBuilder source;
        DialogTypeVisualizer visualizer;
        TypeVisualizerDialog visualizerDialog;
        IDisposable visualizerObserver;

        public VisualizerDialogLauncher(InspectBuilder source, DialogTypeVisualizer visualizer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }

            this.source = source;
            this.visualizer = visualizer;
        }

        public string Text { get; set; }

        public Rectangle Bounds { get; set; }

        public void Show()
        {
            Show(null);
        }

        public void Show(IServiceProvider provider)
        {
            if (visualizerDialog == null)
            {
                using (var visualizerContext = new ServiceContainer(provider))
                {
                    visualizerDialog = new TypeVisualizerDialog();
                    visualizerDialog.Text = Text;
                    visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                    visualizer.Load(visualizerContext);
                    visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));

                    visualizerDialog.Load += delegate
                    {
                        if (!Bounds.IsEmpty) visualizerDialog.DesktopBounds = Bounds;
                        visualizerObserver = source.Output.ObserveOn(visualizerDialog)
                                                   .Subscribe(value => visualizer.Show(value));
                    };

                    visualizerDialog.FormClosing += delegate
                    {
                        Bounds = visualizerDialog.DesktopBounds;
                        visualizerObserver.Dispose();
                    };

                    visualizerDialog.FormClosed += delegate
                    {
                        visualizer.Unload();
                        visualizerDialog.Dispose();
                        visualizerDialog = null;
                    };

                    visualizerDialog.Show();
                }
            }

            visualizerDialog.Activate();
        }

        public void Hide()
        {
            if (visualizerDialog != null)
            {
                visualizerDialog.Close();
            }
        }
    }
}
