using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    internal class VisualizerDialogMap : IEnumerable<VisualizerDialogLauncher>
    {
        readonly TypeVisualizerMap typeVisualizerMap;
        readonly Dictionary<InspectBuilder, VisualizerDialogLauncher> lookup;

        public VisualizerDialogMap(TypeVisualizerMap typeVisualizers)
        {
            typeVisualizerMap = typeVisualizers ?? throw new ArgumentNullException(nameof(typeVisualizers));
            lookup = new();
        }

        public VisualizerDialogLauncher this[InspectBuilder key]
        {
            get => lookup[key];
        }

        public bool TryGetValue(InspectBuilder key, out VisualizerDialogLauncher value)
        {
            return lookup.TryGetValue(key, out value);
        }

        public void Show(VisualizerLayoutMap visualizerSettings, IServiceProvider provider = null, IWin32Window owner = null)
        {
            foreach (var dialogLauncher in lookup.Values)
            {
                var dialogSettings = visualizerSettings[dialogLauncher.Source];
                dialogLauncher.Bounds = dialogSettings.Bounds;
                dialogLauncher.WindowState = dialogSettings.WindowState;
                if (dialogSettings.Visible)
                {
                    dialogLauncher.Show(owner, provider);
                }
            }
        }

        public VisualizerDialogLauncher Add(InspectBuilder source, ExpressionBuilderGraph workflow, VisualizerDialogSettings dialogSettings)
        {
            var dialogLauncher = LayoutHelper.CreateVisualizerLauncher(
                source,
                dialogSettings,
                typeVisualizerMap,
                workflow);
            Add(dialogLauncher);
            return dialogLauncher;
        }

        public void Add(VisualizerDialogLauncher item)
        {
            lookup.Add(item.Source, item);
        }

        public bool Remove(VisualizerDialogLauncher item)
        {
            return lookup.Remove(item.Source);
        }

        public IEnumerator<VisualizerDialogLauncher> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
