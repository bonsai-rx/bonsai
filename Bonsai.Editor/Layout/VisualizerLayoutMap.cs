using System.Collections;
using System.Collections.Generic;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    internal class VisualizerLayoutMap : IEnumerable<VisualizerDialogSettings>
    {
        readonly TypeVisualizerMap typeVisualizerMap;
        readonly Dictionary<InspectBuilder, VisualizerDialogSettings> lookup;

        public VisualizerLayoutMap(TypeVisualizerMap typeVisualizers)
        {
            typeVisualizerMap = typeVisualizers;
            lookup = new();
        }

        public VisualizerDialogSettings this[InspectBuilder key]
        {
            get => lookup[key];
            set => lookup[key] = value;
        }

        public bool TryGetValue(InspectBuilder key, out VisualizerDialogSettings value)
        {
            return lookup.TryGetValue(key, out value);
        }

        private void CreateVisualizerDialogs(ExpressionBuilderGraph workflow, VisualizerDialogMap visualizerDialogs)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                var builder = (InspectBuilder)workflow[i].Value;
                if (lookup.TryGetValue(builder, out VisualizerDialogSettings dialogSettings))
                {
                    visualizerDialogs.Add(builder, workflow, dialogSettings);
                }

                if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder)
                {
                    CreateVisualizerDialogs(workflowBuilder.Workflow, visualizerDialogs);
                }
            }
        }

        public VisualizerDialogMap CreateVisualizerDialogs(WorkflowBuilder workflowBuilder)
        {
            var visualizerDialogs = new VisualizerDialogMap(typeVisualizerMap);
            CreateVisualizerDialogs(workflowBuilder.Workflow, visualizerDialogs);
            return visualizerDialogs;
        }

        public void Update(IEnumerable<VisualizerDialogLauncher> visualizerDialogs)
        {
            var unused = new HashSet<InspectBuilder>(lookup.Keys);
            foreach (var dialog in visualizerDialogs)
            {
                unused.Remove(dialog.Source);
                if (!lookup.TryGetValue(dialog.Source, out VisualizerDialogSettings dialogSettings))
                {
                    dialogSettings = new VisualizerDialogSettings();
                    dialogSettings.Tag = dialog.Source;
                    lookup.Add(dialog.Source, dialogSettings);
                }

                var visible = dialog.Visible;
                dialog.Hide();
                dialogSettings.Visible = visible;
                dialogSettings.Bounds = dialog.Bounds;
                dialogSettings.WindowState = dialog.WindowState;

                var visualizer = dialog.Visualizer.Value;
                var visualizerType = visualizer.GetType();
                if (visualizerType.IsPublic)
                {
                    dialogSettings.VisualizerTypeName = visualizerType.FullName;
                    dialogSettings.VisualizerSettings = LayoutHelper.SerializeVisualizerSettings(
                        visualizer,
                        dialog.Workflow);
                }
            }

            foreach (var builder in unused)
            {
                lookup.Remove(builder);
            }
        }

        public VisualizerLayout GetVisualizerLayout(WorkflowBuilder workflowBuilder)
        {
            return GetVisualizerLayout(workflowBuilder.Workflow);
        }

        private VisualizerLayout GetVisualizerLayout(ExpressionBuilderGraph workflow)
        {
            var layout = new VisualizerLayout();
            for (int i = 0; i < workflow.Count; i++)
            {
                var builder = (InspectBuilder)workflow[i].Value;
                var layoutSettings = new VisualizerDialogSettings { Index = i };

                if (lookup.TryGetValue(builder, out VisualizerDialogSettings dialogSettings))
                {
                    layoutSettings.Visible = dialogSettings.Visible;
                    layoutSettings.Bounds = dialogSettings.Bounds;
                    layoutSettings.WindowState = dialogSettings.WindowState;
                    layoutSettings.VisualizerTypeName = dialogSettings.VisualizerTypeName;
                    layoutSettings.VisualizerSettings = dialogSettings.VisualizerSettings;
                }

                if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder)
                {
                    layoutSettings.NestedLayout = GetVisualizerLayout(workflowBuilder.Workflow);
                }

                if (!layoutSettings.Bounds.IsEmpty ||
                    layoutSettings.VisualizerTypeName != null ||
                    layoutSettings.NestedLayout?.DialogSettings.Count > 0)
                {
                    layout.DialogSettings.Add(layoutSettings);
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
            Clear();
            SetVisualizerLayout(workflowBuilder.Workflow, layout);
        }

        private void SetVisualizerLayout(ExpressionBuilderGraph workflow, VisualizerLayout layout)
        {
            for (int i = 0; i < layout.DialogSettings.Count; i++)
            {
                var layoutSettings = layout.DialogSettings[i];
                var index = layoutSettings.Index.GetValueOrDefault(i);
                if (index < workflow.Count)
                {
                    var builder = (InspectBuilder)workflow[index].Value;
                    var dialogSettings = new VisualizerDialogSettings();
                    dialogSettings.Tag = builder;
                    dialogSettings.Bounds = layoutSettings.Bounds;
                    dialogSettings.WindowState = layoutSettings.WindowState;
                    dialogSettings.Visible = layoutSettings.Visible;
                    dialogSettings.VisualizerTypeName = layoutSettings.VisualizerTypeName;
                    dialogSettings.VisualizerSettings = layoutSettings.VisualizerSettings;
                    Add(dialogSettings);

                    if (layoutSettings.NestedLayout != null &&
                        ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder)
                    {
                        SetVisualizerLayout(workflowBuilder.Workflow, layoutSettings.NestedLayout);
                    }
                }
            }
        }

        public void Add(VisualizerDialogSettings item)
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

        public IEnumerator<VisualizerDialogSettings> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
