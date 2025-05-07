using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    internal class VisualizerWindowMap : IEnumerable<VisualizerWindowLauncher>
    {
        readonly TypeVisualizerMap typeVisualizerMap;
        readonly Dictionary<InspectBuilder, VisualizerWindowLauncher> lookup;

        public VisualizerWindowMap(TypeVisualizerMap typeVisualizers)
        {
            typeVisualizerMap = typeVisualizers ?? throw new ArgumentNullException(nameof(typeVisualizers));
            lookup = new();
        }

        public VisualizerWindowLauncher this[InspectBuilder key]
        {
            get => lookup[key];
        }

        public bool TryGetValue(InspectBuilder key, out VisualizerWindowLauncher value)
        {
            return lookup.TryGetValue(key, out value);
        }

        public void Show(VisualizerLayoutMap visualizerSettings, IServiceProvider provider = null, IWin32Window owner = null)
        {
            foreach (var windowLauncher in lookup.Values)
            {
                var windowSettings = visualizerSettings[windowLauncher.Source];
                windowLauncher.Bounds = windowSettings.Bounds;
                windowLauncher.WindowState = windowSettings.WindowState;
                if (windowSettings.Visible)
                {
                    windowLauncher.Show(owner, provider);
                }
            }
        }

        public VisualizerWindowLauncher Add(InspectBuilder source, ExpressionBuilderGraph workflow, VisualizerWindowSettings windowSettings)
        {
            var windowLauncher = LayoutHelper.CreateVisualizerLauncher(
                source,
                windowSettings,
                typeVisualizerMap,
                workflow);
            Add(windowLauncher);
            return windowLauncher;
        }

        public void Add(VisualizerWindowLauncher item)
        {
            lookup.Add(item.Source, item);
        }

        public bool Remove(VisualizerWindowLauncher item)
        {
            return lookup.Remove(item.Source);
        }

        public IEnumerator<VisualizerWindowLauncher> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
