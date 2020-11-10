using Bonsai.Dag;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    static class LayoutHelper
    {
        const string LayoutExtension = ".layout";
        static readonly XName XsdAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsd";
        static readonly XName XsiAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsi";
        const string XsdAttributeValue = "http://www.w3.org/2001/XMLSchema";
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";

        public static VisualizerDialogSettings GetLayoutSettings(this VisualizerLayout visualizerLayout, object key)
        {
            return visualizerLayout != null
                ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == key || xs.Tag == null)
                : null;
        }

        public static string GetLayoutPath(string fileName)
        {
            return Path.ChangeExtension(fileName, Path.GetExtension(fileName) + LayoutExtension);
        }

        public static void SetWorkflowNotifications(ExpressionBuilderGraph source, bool publishNotifications)
        {
            foreach (var builder in from node in source
                                    let inspectBuilder = node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    select inspectBuilder)
            {
                var inspectBuilder = builder;
                inspectBuilder.PublishNotifications = publishNotifications;
                if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowExpression && workflowExpression.Workflow != null)
                {
                    SetWorkflowNotifications(workflowExpression.Workflow, publishNotifications);
                }
            }
        }

        public static void SetLayoutNotifications(VisualizerLayout root)
        {
            foreach (var settings in root.DialogSettings)
            {
                var inspectBuilder = settings.Tag as InspectBuilder;
                while (inspectBuilder != null && !inspectBuilder.PublishNotifications)
                {
                    inspectBuilder.PublishNotifications = !string.IsNullOrEmpty(settings.VisualizerTypeName);
                    var visualizerElement = ExpressionBuilder.GetVisualizerElement(inspectBuilder);
                    if (inspectBuilder.PublishNotifications && visualizerElement != inspectBuilder)
                    {
                        inspectBuilder = visualizerElement;
                    }
                    else inspectBuilder = null;
                }

                if (settings is WorkflowEditorSettings editorSettings && editorSettings.EditorVisualizerLayout != null)
                {
                    SetLayoutNotifications(editorSettings.EditorVisualizerLayout);
                }
            }
        }

        public static VisualizerDialogLauncher CreateVisualizerLauncher(
            InspectBuilder source,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            ExpressionBuilderGraph workflow,
            Editor.GraphView.WorkflowGraphView workflowGraphView = null)
        {
            var inspectBuilder = ExpressionBuilder.GetVisualizerElement(source);
            if (inspectBuilder.ObservableType == null || !inspectBuilder.PublishNotifications)
            {
                return null;
            }

            Type visualizerType = null;
            var deserializeVisualizer = false;
            Func<DialogTypeVisualizer> visualizerFactory = null;
            var layoutSettings = visualizerLayout.GetLayoutSettings(source);
            var visualizerTypes = typeVisualizerMap.GetTypeVisualizers(inspectBuilder);
            if (layoutSettings != null && !string.IsNullOrEmpty(layoutSettings.VisualizerTypeName))
            {
                visualizerType = visualizerTypes.FirstOrDefault(type => type.FullName == layoutSettings.VisualizerTypeName);
                if (visualizerType != null && layoutSettings.VisualizerSettings != null)
                {
                    var root = layoutSettings.VisualizerSettings;
                    root.SetAttributeValue(XsdAttributeName, XsdAttributeValue);
                    root.SetAttributeValue(XsiAttributeName, XsiAttributeValue);
                    var serializer = new XmlSerializer(visualizerType);
                    using (var reader = layoutSettings.VisualizerSettings.CreateReader())
                    {
                        if (serializer.CanDeserialize(reader))
                        {
                            var visualizer = (DialogTypeVisualizer)serializer.Deserialize(reader);
                            visualizerFactory = () => visualizer;
                            deserializeVisualizer = true;
                        }
                    }
                }
            }

            visualizerType ??= visualizerTypes.FirstOrDefault();
            if (visualizerType == null)
            {
                return null;
            }

            if (visualizerFactory == null)
            {
                var visualizerActivation = Expression.New(visualizerType);
                visualizerFactory = Expression.Lambda<Func<DialogTypeVisualizer>>(visualizerActivation).Compile();
            }

            var launcher = new VisualizerDialogLauncher(inspectBuilder, visualizerFactory, workflow, source, workflowGraphView);
            launcher.Text = source != null ? ExpressionBuilder.GetElementDisplayName(source) : null;
            if (deserializeVisualizer)
            {
                launcher = launcher.Visualizer.Value != null ? launcher : null;
            }
            return launcher;
        }

        public static Dictionary<InspectBuilder, VisualizerDialogLauncher> CreateVisualizerMapping(
            ExpressionBuilderGraph workflow,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            Action<VisualizerDialogLauncher> launchVisualizer = null,
            Editor.GraphView.WorkflowGraphView graphView = null)
        {
            if (workflow == null) return null;
            var visualizerMapping = (from node in workflow.TopologicalSort()
                                     let key = (InspectBuilder)node.Value
                                     let visualizerLauncher = CreateVisualizerLauncher(key, visualizerLayout, typeVisualizerMap, workflow, graphView)
                                     where visualizerLauncher != null
                                     select new { key, visualizerLauncher })
                                     .ToDictionary(mapping => mapping.key,
                                                   mapping => mapping.visualizerLauncher);
            foreach (var mapping in visualizerMapping)
            {
                var key = mapping.Key;
                var visualizerLauncher = mapping.Value;
                var layoutSettings = visualizerLayout.GetLayoutSettings(key);
                if (layoutSettings != null)
                {
                    var visualizer = visualizerLauncher.Visualizer;
                    var mashupVisualizer = visualizer.IsValueCreated ? visualizer.Value as DialogMashupVisualizer : null;
                    if (mashupVisualizer != null)
                    {
                        foreach (var mashup in layoutSettings.Mashups)
                        {
                            if (mashup < 0 || mashup >= visualizerLayout.DialogSettings.Count) continue;
                            var dialogSettings = visualizerLayout.DialogSettings[mashup];
                            var node = workflow.FirstOrDefault(node => node.Value == dialogSettings.Tag);
                            if (node?.Value is InspectBuilder inspectBuilder &&
                                visualizerMapping.TryGetValue(inspectBuilder, out VisualizerDialogLauncher visualizerDialog))
                            {
                                visualizerLauncher.CreateMashup(visualizerDialog, typeVisualizerMap);
                            }
                        }
                    }

                    visualizerLauncher.Bounds = layoutSettings.Bounds;
                    visualizerLauncher.WindowState = layoutSettings.WindowState;
                    if (layoutSettings.Visible && launchVisualizer != null)
                    {
                        launchVisualizer(visualizerLauncher);
                    }
                }
            }

            return visualizerMapping;
        }

        public static XElement SerializeVisualizerSettings(DialogTypeVisualizer visualizer)
        {
            var visualizerType = visualizer.GetType();
            var visualizerSettings = new XDocument();
            var serializer = new XmlSerializer(visualizerType);
            using (var writer = visualizerSettings.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }
            var root = visualizerSettings.Root;
            root.Remove();
            var xsdAttribute = root.Attribute(XsdAttributeName);
            if (xsdAttribute != null) xsdAttribute.Remove();
            var xsiAttribute = root.Attribute(XsiAttributeName);
            if (xsiAttribute != null) xsiAttribute.Remove();
            return root;
        }
    }
}
