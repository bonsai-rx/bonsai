using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.ComponentModel.Design;
using System.Reactive.Linq;

namespace Bonsai.Design
{
    public static class InspectBuilderExtensions
    {
        public static Action<bool> CreateVisualizerDialog(this InspectBuilder source, string caption, DialogTypeVisualizer visualizer, IServiceProvider provider)
        {
            TypeVisualizerDialog visualizerDialog = null;
            IDisposable visualizerObserver = null;

            return show =>
            {
                if (show)
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

                            visualizerDialog.Load += delegate
                            {
                                visualizerObserver = source.Output.ObserveOn(visualizerDialog)
                                                           .Subscribe(value => visualizer.Show(value));
                            };
                            visualizerDialog.FormClosing += delegate { visualizerObserver.Dispose(); };
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
                else if (visualizerDialog != null)
                {
                    visualizerDialog.Close();
                }
            };
        }
    }
}
