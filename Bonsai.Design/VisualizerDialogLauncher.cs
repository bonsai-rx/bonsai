using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class VisualizerDialogLauncher : DialogLauncher
    {
        InspectBuilder source;
        DialogTypeVisualizer visualizer;
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

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            using (var visualizerContext = new ServiceContainer(provider))
            {
                visualizerDialog.Text = Text;
                visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                visualizer.Load(visualizerContext);
                visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));

                visualizerDialog.Load += delegate
                {
                    visualizerObserver = source.Output.ObserveOn(visualizerDialog)
                                                      .Subscribe(value => visualizer.Show(value));
                };

                visualizerDialog.FormClosing += delegate { visualizerObserver.Dispose(); };
                visualizerDialog.FormClosed += delegate { visualizer.Unload(); };
            }
        }
    }
}
