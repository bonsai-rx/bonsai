using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.ComponentModel.Design;

namespace Bonsai.Design
{
    public static class InspectBuilderExtensions
    {
        public static Action CreateVisualizer(this InspectBuilder source, string caption, DialogTypeVisualizer visualizer, IServiceProvider provider)
        {
            TypeVisualizerDialog visualizerDialog = null;
            IDisposable visualizerObserver = null;

            return () =>
            {
                if (visualizerDialog == null)
                {
                    using (var visualizerContext = new ServiceContainer(provider))
                    {
                        visualizerDialog = new TypeVisualizerDialog();
                        visualizerDialog.Text = caption;
                        visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                        visualizer.Load(visualizerContext);
                        visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));

                        visualizerDialog.FormClosing += delegate { visualizerObserver.Dispose(); };
                        visualizerDialog.FormClosed += delegate
                        {
                            visualizer.Unload();
                            visualizerDialog = null;
                        };

                        visualizerObserver = source.Output.Subscribe(value => visualizer.Show(value));
                        visualizerDialog.Show();
                    }
                }

                visualizerDialog.Activate();
            };
        }
    }
}
