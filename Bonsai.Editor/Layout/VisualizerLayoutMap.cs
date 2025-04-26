using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Editor;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    internal class VisualizerLayoutMap : IEnumerable<VisualizerWindowSettings>
    {
        readonly TypeVisualizerMap typeVisualizerMap;
        Dictionary<InspectBuilder, VisualizerWindowSettings> lookup;

        public VisualizerLayoutMap(TypeVisualizerMap typeVisualizers)
        {
            typeVisualizerMap = typeVisualizers;
            lookup = new();
        }

        public VisualizerWindowSettings this[InspectBuilder key]
        {
            get => lookup[key];
            set => lookup[key] = value;
        }

        public bool TryGetValue(InspectBuilder key, out VisualizerWindowSettings value)
        {
            return lookup.TryGetValue(key, out value);
        }

        private void CreateVisualizerWindows(ExpressionBuilderGraph workflow, VisualizerWindowMap visualizerWindows)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                var source = (InspectBuilder)workflow[i].Value;
                if (source.ObservableType is null) continue;

                if (source.Builder is VisualizerWindow visualizerWindow)
                {
                    if (!lookup.TryGetValue(source, out VisualizerWindowSettings cachedSettings))
                        cachedSettings = new();

                    lookup[source] = new VisualizerWindowSettings
                    {
                        Visible = visualizerWindow.Visible.GetValueOrDefault(cachedSettings.Visible),
                        Location = visualizerWindow.Location.GetValueOrDefault(cachedSettings.Location),
                        Size = visualizerWindow.Size.GetValueOrDefault(cachedSettings.Size),
                        WindowState = visualizerWindow.WindowState.GetValueOrDefault(cachedSettings.WindowState),
                        VisualizerTypeName = visualizerWindow.VisualizerType?
                            .GetType()
                            .GetGenericArguments()[0]
                            .FullName
                    };
                }

                if (lookup.TryGetValue(source, out VisualizerWindowSettings windowSettings))
                {
                    visualizerWindows.Add(source, workflow, windowSettings);
                }

                if (source.Builder is IWorkflowExpressionBuilder workflowBuilder)
                {
                    CreateVisualizerWindows(workflowBuilder.Workflow, visualizerWindows);
                }
            }
        }

        public VisualizerWindowMap CreateVisualizerWindows(WorkflowBuilder workflowBuilder)
        {
            var visualizerWindows = new VisualizerWindowMap(typeVisualizerMap);
            CreateVisualizerWindows(workflowBuilder.Workflow, visualizerWindows);
            return visualizerWindows;
        }

        public void Update(IEnumerable<VisualizerWindowLauncher> visualizerWindows)
        {
            var unused = new HashSet<InspectBuilder>(lookup.Keys);
            foreach (var window in visualizerWindows)
            {
                unused.Remove(window.Source);
                if (!lookup.TryGetValue(window.Source, out VisualizerWindowSettings windowSettings))
                {
                    windowSettings = new VisualizerWindowSettings();
                    windowSettings.Tag = window.Source;
                    lookup.Add(window.Source, windowSettings);
                }

                var visible = window.Visible;
                window.Hide();
                windowSettings.Visible = visible;
                windowSettings.Bounds = window.Bounds;
                windowSettings.WindowState = window.WindowState;

                var visualizer = window.Visualizer.Value;
                var visualizerType = visualizer.GetType();
                if (visualizerType.IsPublic && window.Source.Builder is not VisualizerWindow)
                {
                    windowSettings.VisualizerTypeName = visualizerType.FullName;
                    windowSettings.VisualizerSettings = LayoutHelper.SerializeVisualizerSettings(
                        visualizer,
                        window.Workflow);
                }
            }

            foreach (var builder in unused)
            {
                lookup.Remove(builder);
            }
        }

        public VisualizerLayout GetVisualizerLayout(WorkflowBuilder workflowBuilder)
        {
            var layout = GetVisualizerLayout(workflowBuilder.Workflow);
            layout.Version = AboutBox.AssemblyVersion;
            return layout;
        }

        private VisualizerLayout GetVisualizerLayout(ExpressionBuilderGraph workflow)
        {
            var layout = new VisualizerLayout();
            for (int i = 0; i < workflow.Count; i++)
            {
                var builder = (InspectBuilder)workflow[i].Value;
                var layoutSettings = new VisualizerWindowSettings { Index = i };

                if (lookup.TryGetValue(builder, out VisualizerWindowSettings windowSettings))
                {
                    layoutSettings.Visible = windowSettings.Visible;
                    layoutSettings.Bounds = windowSettings.Bounds;
                    layoutSettings.WindowState = windowSettings.WindowState;
                    layoutSettings.VisualizerTypeName = windowSettings.VisualizerTypeName;
                    layoutSettings.VisualizerSettings = windowSettings.VisualizerSettings;
                }

                if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder &&
                    workflowBuilder.Workflow is not null)
                {
                    layoutSettings.NestedLayout = GetVisualizerLayout(workflowBuilder.Workflow);
                }

                if (!layoutSettings.Bounds.IsEmpty ||
                    layoutSettings.VisualizerTypeName != null ||
                    layoutSettings.NestedLayout?.WindowSettings.Count > 0)
                {
                    layout.WindowSettings.Add(layoutSettings);
                }
            }

            return layout;
        }

        public static VisualizerLayoutMap FromVisualizerLayout(
            WorkflowBuilder workflowBuilder,
            VisualizerLayout layout,
            TypeVisualizerMap typeVisualizers)
        {
            var visualizerSettings = new VisualizerLayoutMap(typeVisualizers);
            visualizerSettings.SetVisualizerLayout(workflowBuilder.Workflow, layout);
            return visualizerSettings;
        }

        public void SetVisualizerLayout(WorkflowBuilder workflowBuilder, VisualizerLayout layout)
        {
            var visualizerSettings = FromVisualizerLayout(workflowBuilder, layout, typeVisualizerMap);
            lookup = visualizerSettings.lookup;
        }

        private void SetVisualizerLayout(ExpressionBuilderGraph workflow, VisualizerLayout layout)
        {
            for (int i = 0; i < layout.WindowSettings.Count; i++)
            {
                var layoutSettings = layout.WindowSettings[i];
                var index = layoutSettings.Index.GetValueOrDefault(i);
                if (index < 0 || index >= workflow.Count)
                    throw new InvalidOperationException($"Element #{index} does not exist in the workflow.");
                else
                {
                    var builder = (InspectBuilder)workflow[index].Value;
                    var windowSettings = new VisualizerWindowSettings();
                    windowSettings.Tag = builder;
                    windowSettings.Bounds = layoutSettings.Bounds;
                    windowSettings.WindowState = layoutSettings.WindowState;
                    windowSettings.Visible = layoutSettings.Visible;
                    windowSettings.VisualizerSettings = layoutSettings.VisualizerSettings;
                    if (!string.IsNullOrEmpty(layoutSettings.VisualizerTypeName))
                    {
                        if (typeVisualizerMap.GetVisualizerType(layoutSettings.VisualizerTypeName) is null)
                            throw new InvalidOperationException(
                                $"Visualizer cannot be applied to element #{index}: " +
                                $"{ExpressionBuilder.GetWorkflowElement(builder).GetType()}. The visualizer type " +
                                $"'{layoutSettings.VisualizerTypeName}' is not available.");

                        var visualizerElement = ExpressionBuilder.GetVisualizerElement(builder);
                        var visualizerTypes = typeVisualizerMap.GetTypeVisualizers(visualizerElement);
                        if (!visualizerTypes.Any(type => type.FullName == layoutSettings.VisualizerTypeName))
                            throw new InvalidOperationException(
                                $"Visualizer type '{layoutSettings.VisualizerTypeName}' cannot be applied " +
                                $"to element #{index}: {ExpressionBuilder.GetWorkflowElement(builder).GetType()}.");
                        windowSettings.VisualizerTypeName = layoutSettings.VisualizerTypeName;
                    }
                    Add(windowSettings);

                    if (layoutSettings.NestedLayout != null &&
                        ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder)
                    {
                        try { SetVisualizerLayout(workflowBuilder.Workflow, layoutSettings.NestedLayout); }
                        catch (InvalidOperationException innerException)
                        {
                            throw new InvalidOperationException(
                                $"Visualizer cannot be applied to an inner element of nested layout #{index}: " +
                                $"{ExpressionBuilder.GetWorkflowElement(builder).GetType()}.",
                                innerException);
                        }
                    }
                }
            }
        }

        public void Add(VisualizerWindowSettings item)
        {
            var builder = (InspectBuilder)item.Tag;
            lookup.Add(builder, item);
        }

        public bool ContainsKey(InspectBuilder builder)
        {
            return lookup.ContainsKey(builder);
        }

        public bool Remove(InspectBuilder builder)
        {
            return lookup.Remove(builder);
        }

        public void Clear()
        {
            lookup.Clear();
        }

        public IEnumerator<VisualizerWindowSettings> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
