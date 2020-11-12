using System;
using System.Linq;

namespace Bonsai.Editor
{
    [Obsolete]
    public class MainForm : EditorForm
    {
        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider)
            : base(elementProvider, visualizerProvider)
        {
        }

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            IServiceProvider provider)
            : base(elementProvider, visualizerProvider, provider)
        {
        }

        public MainForm(
            IObservable<IGrouping<string, WorkflowElementDescriptor>> elementProvider,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            IServiceProvider provider,
            float editorScale)
            : base(elementProvider, visualizerProvider, provider, editorScale)
        {
        }
    }
}
