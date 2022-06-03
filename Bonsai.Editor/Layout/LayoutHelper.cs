using Bonsai.Dag;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
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
        const string MashupSourceElement = "Mashup";
        const string MashupSourceAttribute = "Source";

        public static VisualizerDialogSettings GetLayoutSettings(this VisualizerLayout visualizerLayout, object key)
        {
            return visualizerLayout?.DialogSettings.FirstOrDefault(xs => xs.Tag == key || xs.Tag == null);
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
                SetLayoutNotifications(settings, root, publishNotifications: false);
            }
        }

        static void SetLayoutNotifications(VisualizerDialogSettings settings, VisualizerLayout root, bool publishNotifications = false)
        {
            var inspectBuilder = settings.Tag as InspectBuilder;
            while (inspectBuilder != null && !inspectBuilder.PublishNotifications)
            {
                inspectBuilder.PublishNotifications = publishNotifications || !string.IsNullOrEmpty(settings.VisualizerTypeName);
                foreach (var index in settings.Mashups.Concat(settings.VisualizerSettings?
                                                      .Descendants(MashupSourceElement)
                                                      .Select(m => m.Attribute(MashupSourceAttribute)?.Value)
                                                      .Where(attr => attr != null)
                                                      .Select(int.Parse)
                                                      .Distinct() ?? Enumerable.Empty<int>()))
                {
                    if (index < 0 || index >= root.DialogSettings.Count) continue;
                    var mashupSource = root.DialogSettings[index];
                    SetLayoutNotifications(mashupSource, root, publishNotifications: true);
                }

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

            var deserializeVisualizer = false;
            Func<DialogTypeVisualizer> visualizerFactory = null;
            var layoutSettings = visualizerLayout.GetLayoutSettings(source);
            var visualizer = DeserializeVisualizerSettings(inspectBuilder, layoutSettings, visualizerLayout, typeVisualizerMap);
            if (visualizer != null)
            {
                visualizerFactory = () => visualizer;
                deserializeVisualizer = true;
            }

            var visualizerType = visualizer?.GetType() ?? typeVisualizerMap.GetTypeVisualizers(inspectBuilder).FirstOrDefault();
            if (visualizerType == null)
            {
                return null;
            }

            if (visualizerFactory == null)
            {
                var visualizerActivation = Expression.New(visualizerType);
                visualizerFactory = Expression.Lambda<Func<DialogTypeVisualizer>>(visualizerActivation).Compile();
            }

            var launcher = new VisualizerDialogLauncher(visualizerType, inspectBuilder, visualizerFactory, workflow, source, workflowGraphView);
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
            IServiceProvider provider = null,
            IWin32Window owner = null,
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
                    visualizerLauncher.Bounds = layoutSettings.Bounds;
                    visualizerLauncher.WindowState = layoutSettings.WindowState;
                    if (layoutSettings.Visible)
                    {
                        visualizerLauncher.Show(owner, provider);
                    }
                }
            }

            return visualizerMapping;
        }

        public static XElement SerializeVisualizerSettings(
            DialogTypeVisualizer visualizer,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            var visualizerType = visualizer.GetType();
            var visualizerSettings = new XDocument();
            var serializer = new XmlSerializer(visualizerType);
            using (var writer = visualizerSettings.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }
            var root = visualizerSettings.Root;
            if (visualizer is MashupVisualizer mashupVisualizer)
            {
                SerializeMashupVisualizerSettings(root, mashupVisualizer, topologicalOrder);
            }
            root.Remove();
            var xsdAttribute = root.Attribute(XsdAttributeName);
            if (xsdAttribute != null) xsdAttribute.Remove();
            var xsiAttribute = root.Attribute(XsiAttributeName);
            if (xsiAttribute != null) xsiAttribute.Remove();
            return root;
        }

        static void SerializeMashupVisualizerSettings(
            XElement root,
            MashupVisualizer mashupVisualizer,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            foreach (var source in mashupVisualizer.MashupSources)
            {
                var sourceIndex = GetMashupSourceIndex(source, topologicalOrder);
                if (sourceIndex.HasValue)
                {
                    var mashupSource = SerializeMashupSource(sourceIndex.Value, source.Visualizer, topologicalOrder);
                    root.Add(mashupSource);
                }
            }
        }

        static XElement SerializeMashupSource(
            int sourceIndex,
            DialogTypeVisualizer visualizer,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            var visualizerSettings = new XDocument();
            var visualizerType = visualizer.GetType();
            var serializer = new XmlSerializer(visualizerType);
            using (var writer = visualizerSettings.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }

            if (visualizer is MashupVisualizer mashupVisualizer)
            {
                SerializeMashupVisualizerSettings(visualizerSettings.Root, mashupVisualizer, topologicalOrder);
            }

            visualizerSettings = new XDocument(
                new XElement(MashupSourceElement,
                new XAttribute(MashupSourceAttribute, sourceIndex),
                new XElement(nameof(VisualizerDialogSettings.VisualizerSettings),
                visualizerSettings.Root)));
            visualizerSettings.Root.AddFirst(new XElement(
                nameof(VisualizerDialogSettings.VisualizerTypeName),
                visualizerType.FullName));
            return visualizerSettings.Root;
        }

        public static DialogTypeVisualizer DeserializeVisualizerSettings(
            InspectBuilder source,
            VisualizerDialogSettings layoutSettings,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap)
        {
            return DeserializeMashupSource(
                source,
                layoutSettings?.VisualizerTypeName,
                layoutSettings?.VisualizerSettings,
                visualizerLayout,
                typeVisualizerMap);
        }

        static DialogTypeVisualizer DeserializeMashupSource(
            InspectBuilder source,
            string visualizerTypeName,
            XElement visualizerSettings,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            MashupVisualizer container = null)
        {
            Type visualizerType = default;
            if (!string.IsNullOrEmpty(visualizerTypeName))
            {
                visualizerType = typeVisualizerMap.GetVisualizerType(visualizerTypeName);
                if (visualizerType != null && visualizerSettings != null)
                {
                    visualizerSettings.SetAttributeValue(XsdAttributeName, XsdAttributeValue);
                    visualizerSettings.SetAttributeValue(XsiAttributeName, XsiAttributeValue);
                    var serializer = new XmlSerializer(visualizerType);
                    using (var reader = visualizerSettings.CreateReader())
                    {
                        if (serializer.CanDeserialize(reader))
                        {
                            var visualizer = (DialogTypeVisualizer)serializer.Deserialize(reader);
                            if (visualizer is MashupVisualizer mashupContainer)
                            {
                                var mashupSources = visualizerSettings.Elements(MashupSourceElement);
                                foreach (var mashupSource in mashupSources)
                                {
                                    var mashupIndexAttribute = mashupSource.Attribute(MashupSourceAttribute);
                                    if (mashupIndexAttribute == null) continue;

                                    var mashupIndex = int.Parse(mashupIndexAttribute.Value);
                                    var mashupSourceBuilder = (InspectBuilder)visualizerLayout.DialogSettings[mashupIndex]?.Tag;
                                    var mashupVisualizerTypeName = mashupSource.Element(nameof(VisualizerDialogSettings.VisualizerTypeName))?.Value;
                                    var mashupVisualizerSettings = mashupSource.Element(nameof(VisualizerDialogSettings.VisualizerSettings));
                                    DeserializeMashupSource(
                                        mashupSourceBuilder,
                                        mashupVisualizerTypeName,
                                        mashupVisualizerSettings.Elements().FirstOrDefault(),
                                        visualizerLayout,
                                        typeVisualizerMap,
                                        mashupContainer);
                                }
                            }

                            if (container != null)
                            {
                                var visualizerMashup = new MashupSource(source, visualizer);
                                container.MashupSources.Add(visualizerMashup);
                            }
                            return visualizer;
                        }
                    }
                }
            }

            return null;
        }

        static int? GetMashupSourceIndex(
            ITypeVisualizerContext mashup,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            return topologicalOrder
                .Select((n, i) => ExpressionBuilder.GetVisualizerElement(n.Value) == mashup.Source ? (int?)i : null)
                .FirstOrDefault(index => index.HasValue);
        }
    }
}
