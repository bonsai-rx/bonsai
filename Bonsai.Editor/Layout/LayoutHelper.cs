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
        const string MashupSettingsElement = "MashupSettings";
        const string IntElement = "int";

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
                foreach (var mashup in settings.Mashups.Concat(settings.VisualizerSettings?
                                                       .Descendants(nameof(settings.Mashups))
                                                       .SelectMany(m => m.Elements(IntElement).Select(e => int.Parse(e.Value)))
                                                       .Distinct() ?? Enumerable.Empty<int>()))
                {
                    if (mashup < 0 || mashup >= root.DialogSettings.Count) continue;
                    var dialogSettings = root.DialogSettings[mashup];
                    SetLayoutNotifications(dialogSettings, root, publishNotifications: true);
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
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder,
            IList<int> mashups)
        {
            var visualizerType = visualizer.GetType();
            var visualizerSettings = new XDocument();
            var serializer = new XmlSerializer(visualizerType);
            using (var writer = visualizerSettings.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }
            var root = visualizerSettings.Root;
            if (visualizer is DialogMashupVisualizer mashupVisualizer)
            {
                SerializeMashupVisualizerSettings(root, mashupVisualizer, topologicalOrder, mashups);
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
            DialogMashupVisualizer mashupVisualizer,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder,
            IList<int> mashups)
        {
            foreach (var mashup in mashupVisualizer.Mashups)
            {
                var mashupIndex = GetMashupIndex(mashup, topologicalOrder);
                if (mashupIndex.HasValue)
                {
                    var mashupSettings = SerializeMashupSettings(mashup.Visualizer, topologicalOrder);
                    root.Add(mashupSettings);
                    mashups.Add(mashupIndex.Value);
                }
            }
        }

        static XElement SerializeMashupSettings(
            DialogTypeVisualizer visualizer,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            var visualizerSettings = new XDocument();
            if (visualizer is MashupVisualizerAdapter mashupSettings)
            {
                visualizer = mashupSettings.Visualizer;
            }

            var visualizerType = visualizer.GetType();
            var serializer = new XmlSerializer(visualizerType);
            using (var writer = visualizerSettings.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }

            List<int> mashups = default;
            if (visualizer is DialogMashupVisualizer mashupVisualizer)
            {
                mashups = new List<int>();
                SerializeMashupVisualizerSettings(visualizerSettings.Root, mashupVisualizer, topologicalOrder, mashups);
            }

            visualizerSettings = new XDocument(
                new XElement(MashupSettingsElement,
                new XElement(nameof(VisualizerDialogSettings.VisualizerSettings),
                visualizerSettings.Root)));
            visualizerSettings.Root.AddFirst(new XElement(
                nameof(VisualizerDialogSettings.VisualizerTypeName),
                visualizerType.FullName));
            if (mashups != null)
            {
                visualizerSettings.Root.Add(new XElement(
                    nameof(VisualizerDialogSettings.Mashups),
                    mashups.Select(index => new XElement(IntElement, index)).ToArray()));
            }
            return visualizerSettings.Root;
        }

        public static DialogTypeVisualizer DeserializeVisualizerSettings(
            InspectBuilder source,
            VisualizerDialogSettings layoutSettings,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap)
        {
            return DeserializeMashupSettings(
                source,
                layoutSettings?.VisualizerTypeName,
                layoutSettings?.VisualizerSettings,
                layoutSettings?.Mashups,
                visualizerLayout,
                typeVisualizerMap);
        }

        static DialogTypeVisualizer DeserializeMashupSettings(
            InspectBuilder source,
            string visualizerTypeName,
            XElement visualizerSettings,
            IEnumerable<int> mashupIndices,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            DialogMashupVisualizer container = null)
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
                            if (visualizer is DialogMashupVisualizer mashupContainer)
                            {
                                var mashupSettings = visualizerSettings.Elements(MashupSettingsElement);
                                foreach (var mashup in mashupSettings.Zip(mashupIndices, (element, i) => (element, i)))
                                {
                                    var mashupSource = (InspectBuilder)visualizerLayout.DialogSettings[mashup.i]?.Tag;
                                    var mashupVisualizerTypeName = mashup.element.Element(nameof(VisualizerDialogSettings.VisualizerTypeName))?.Value;
                                    var mashupVisualizerSettings = mashup.element.Element(nameof(VisualizerDialogSettings.VisualizerSettings));
                                    var nestedMashups = mashup.element
                                        .Element(nameof(VisualizerDialogSettings.Mashups))?
                                        .Elements(IntElement)
                                        .Select(node => int.Parse(node.Value));
                                    DeserializeMashupSettings(
                                        mashupSource,
                                        mashupVisualizerTypeName,
                                        mashupVisualizerSettings.Elements().FirstOrDefault(),
                                        nestedMashups,
                                        visualizerLayout,
                                        typeVisualizerMap,
                                        mashupContainer);
                                }
                            }

                            if (container != null)
                            {
                                var mashupVisualizer = visualizer as MashupTypeVisualizer;
                                if (mashupVisualizer == null)
                                {
                                    mashupVisualizer = (MashupTypeVisualizer)Activator.CreateInstance(typeof(MashupVisualizerAdapter), visualizer);
                                }

                                var visualizerMashup = new VisualizerMashup(source, mashupVisualizer);
                                container.Mashups.Add(visualizerMashup);
                            }
                            return visualizer;
                        }
                    }
                }
            }

            return null;
        }

        static int? GetMashupIndex(
            ITypeVisualizerContext mashup,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            return topologicalOrder
                .Select((n, i) => ExpressionBuilder.GetVisualizerElement(n.Value) == mashup.Source ? (int?)i : null)
                .FirstOrDefault(index => index.HasValue);
        }
    }
}
