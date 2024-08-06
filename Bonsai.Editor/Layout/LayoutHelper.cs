using Bonsai.Dag;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    static class LayoutHelper
    {
        static readonly XName XsdAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsd";
        static readonly XName XsiAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsi";
        const string XsdAttributeValue = "http://www.w3.org/2001/XMLSchema";
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";
        const string MashupSettingsElement = "MashupSettings";
        const string MashupSourceElement = "Source";

        public static VisualizerDialogSettings GetLayoutSettings(this VisualizerLayout visualizerLayout, object key)
        {
            return visualizerLayout?.DialogSettings.FirstOrDefault(xs => xs.Tag == key || xs.Tag == null);
        }

        [Obsolete]
        public static string GetLayoutPath(string fileName)
        {
            var newLayoutPath = Editor.Project.GetLayoutConfigPath(fileName);
            return File.Exists(newLayoutPath)
                ? newLayoutPath
                : Editor.Project.GetLegacyLayoutConfigPath(fileName);
        }

        public static void SetLayoutTags(ExpressionBuilderGraph source, VisualizerLayout layout)
        {
            foreach (var node in source)
            {
                var builder = node.Value;
                var layoutSettings = layout.GetLayoutSettings(builder);
                if (layoutSettings == null)
                {
                    layoutSettings = new VisualizerDialogSettings();
                    layout.DialogSettings.Add(layoutSettings);
                }
                layoutSettings.Tag = builder;

                if (layoutSettings is WorkflowEditorSettings editorSettings &&
                    ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowBuilder &&
                    editorSettings.EditorVisualizerLayout != null &&
                    editorSettings.EditorDialogSettings.Visible &&
                    workflowBuilder.Workflow != null)
                {
                    SetLayoutTags(workflowBuilder.Workflow, editorSettings.EditorVisualizerLayout);
                }
            }
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
                SetLayoutNotifications(settings, root, forcePublish: false);
            }
        }

        static void SetLayoutNotifications(VisualizerDialogSettings settings, VisualizerLayout root, bool forcePublish = false)
        {
            var inspectBuilder = settings.Tag as InspectBuilder;
            while (inspectBuilder != null && !inspectBuilder.PublishNotifications)
            {
                if (string.IsNullOrEmpty(settings.VisualizerTypeName) && !forcePublish)
                {
                    break;
                }

                SetVisualizerNotifications(inspectBuilder);
                foreach (var index in settings.Mashups.Concat(settings.VisualizerSettings?
                                                      .Descendants(MashupSourceElement)
                                                      .Select(m => int.Parse(m.Value))
                                                      .Distinct() ?? Enumerable.Empty<int>()))
                {
                    if (index < 0 || index >= root.DialogSettings.Count) continue;
                    var mashupSource = root.DialogSettings[index];
                    SetLayoutNotifications(mashupSource, root, forcePublish: true);
                }

                inspectBuilder = ExpressionBuilder.GetVisualizerElement(inspectBuilder);
            }

            if (settings is WorkflowEditorSettings editorSettings && editorSettings.EditorVisualizerLayout != null)
            {
                SetLayoutNotifications(editorSettings.EditorVisualizerLayout);
            }
        }

        static void SetVisualizerNotifications(InspectBuilder inspectBuilder)
        {
            inspectBuilder.PublishNotifications = true;
            foreach (var visualizerMapping in ExpressionBuilder.GetVisualizerMappings(inspectBuilder))
            {
                SetVisualizerNotifications(visualizerMapping.Source);
            }
        }

        static IEnumerable<VisualizerFactory> GetMashupSources(this VisualizerFactory visualizerFactory)
        {
            yield return visualizerFactory;
            foreach (var source in visualizerFactory.MashupSources)
            {
                foreach (var nestedSource in source.GetMashupSources())
                {
                    yield return nestedSource;
                }
            }
        }

        internal static Type GetMashupSourceType(Type mashupVisualizerType, Type visualizerType, TypeVisualizerMap typeVisualizerMap)
        {
            Type mashupSource = default;
            while (mashupVisualizerType != null && mashupVisualizerType != typeof(MashupVisualizer))
            {
                var mashup = typeof(MashupSource<,>).MakeGenericType(mashupVisualizerType, visualizerType);
                mashupSource = typeVisualizerMap.GetTypeVisualizers(mashup).FirstOrDefault();
                if (mashupSource != null) break;

                mashup = typeof(MashupSource<>).MakeGenericType(mashupVisualizerType);
                mashupSource = typeVisualizerMap.GetTypeVisualizers(mashup).FirstOrDefault();
                if (mashupSource != null) break;
                mashupVisualizerType = mashupVisualizerType.BaseType;
            }

            if (mashupSource != null && mashupSource.IsGenericTypeDefinition)
            {
                mashupSource = mashupSource.MakeGenericType(visualizerType);
            }
            return mashupSource;
        }

        public static VisualizerDialogLauncher CreateVisualizerLauncher(
            InspectBuilder source,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            ExpressionBuilderGraph workflow,
            IReadOnlyList<VisualizerFactory> mashupArguments,
            Editor.GraphView.WorkflowGraphView workflowGraphView = null)
        {
            var inspectBuilder = ExpressionBuilder.GetVisualizerElement(source);
            if (inspectBuilder.ObservableType == null || !inspectBuilder.PublishNotifications ||
                source.Builder is VisualizerMappingBuilder)
            {
                return null;
            }

            var layoutSettings = visualizerLayout.GetLayoutSettings(source);
            var visualizerType = typeVisualizerMap.GetVisualizerType(layoutSettings?.VisualizerTypeName ?? string.Empty)
                                 ?? typeVisualizerMap.GetTypeVisualizers(inspectBuilder).FirstOrDefault();
            if (visualizerType == null)
            {
                return null;
            }

            var visualizerFactory = new VisualizerFactory(inspectBuilder, visualizerType, mashupArguments);
            var visualizer = new Lazy<DialogTypeVisualizer>(() => DeserializeVisualizerSettings(
                visualizerType,
                layoutSettings,
                visualizerLayout,
                visualizerFactory,
                typeVisualizerMap));

            var launcher = new VisualizerDialogLauncher(visualizer, visualizerFactory, workflow, source, workflowGraphView);
            launcher.Text = source != null ? ExpressionBuilder.GetElementDisplayName(source) : null;
            return launcher;
        }

        static IReadOnlyList<VisualizerFactory> GetMashupArguments(InspectBuilder builder, TypeVisualizerMap typeVisualizerMap)
        {
            var visualizerMappings = ExpressionBuilder.GetVisualizerMappings(builder);
            if (visualizerMappings.Count == 0) return Array.Empty<VisualizerFactory>();
            return visualizerMappings.Select(mapping =>
            {
                // stack overflow happens if a visualizer ends up being mapped to itself
                if (mapping.Source == builder)
                    throw new WorkflowBuildException("Combining together visualizer mappings from the same node is not currently supported.", builder);

                var nestedSources = GetMashupArguments(mapping.Source, typeVisualizerMap);
                var visualizerType = mapping.VisualizerType ?? typeVisualizerMap.GetTypeVisualizers(mapping.Source).FirstOrDefault();
                return new VisualizerFactory(mapping.Source, visualizerType, nestedSources);
            }).ToList();
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
                                     let source = (InspectBuilder)node.Value
                                     let mashupArguments = GetMashupArguments(source, typeVisualizerMap)
                                     let visualizerLauncher = CreateVisualizerLauncher(
                                         source,
                                         visualizerLayout,
                                         typeVisualizerMap,
                                         workflow,
                                         mashupArguments,
                                         graphView)
                                     where visualizerLauncher != null
                                     select new { source, visualizerLauncher })
                                     .ToDictionary(mapping => mapping.source,
                                                   mapping => mapping.visualizerLauncher);
            foreach (var mapping in visualizerMapping)
            {
                var key = mapping.Key;
                var visualizerLauncher = mapping.Value;
                var layoutSettings = visualizerLayout.GetLayoutSettings(key);
                if (layoutSettings != null)
                {
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
                var mashupSource = SerializeMashupSource(sourceIndex, source.Visualizer, topologicalOrder);
                root.Add(mashupSource);
            }
        }

        static XElement SerializeMashupSource(
            int? sourceIndex,
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
                new XElement(MashupSettingsElement,
                sourceIndex.HasValue ? new XElement(MashupSourceElement, sourceIndex.Value) : null,
                new XElement(nameof(VisualizerDialogSettings.VisualizerTypeName), visualizerType.FullName),
                new XElement(nameof(VisualizerDialogSettings.VisualizerSettings), visualizerSettings.Root)));
            return visualizerSettings.Root;
        }

        public static DialogTypeVisualizer DeserializeVisualizerSettings(
            Type visualizerType,
            VisualizerDialogSettings layoutSettings,
            VisualizerLayout visualizerLayout,
            VisualizerFactory visualizerFactory,
            TypeVisualizerMap typeVisualizerMap)
        {
            if (layoutSettings?.VisualizerTypeName != visualizerType.FullName)
            {
                layoutSettings = default;
            }

            if (layoutSettings != null && layoutSettings.Mashups.Count > 0)
            {
                var mashupSettings = layoutSettings.VisualizerSettings.Elements(MashupSettingsElement);
                foreach (var mashup in mashupSettings.Zip(layoutSettings.Mashups, (element, index) => (element, index)))
                {
                    mashup.element.AddFirst(new XElement(MashupSourceElement, mashup.index));
                    var visualizerSettings = mashup.element.Element(nameof(VisualizerDialogSettings.VisualizerSettings));
                    var visualizerTypeName = mashup.element.Element(nameof(VisualizerDialogSettings.VisualizerTypeName))?.Value;
                    if (visualizerSettings != null && visualizerTypeName != null)
                    {
                        visualizerSettings.Remove();
                        visualizerSettings.Name = visualizerTypeName.Split('.').LastOrDefault();
                        mashup.element.Add(new XElement(nameof(VisualizerDialogSettings.VisualizerSettings), visualizerSettings));
                    }
                }
                layoutSettings.Mashups.Clear();
            }

            return visualizerFactory.CreateVisualizer(layoutSettings?.VisualizerSettings, visualizerLayout, typeVisualizerMap);
        }

        static int? GetMashupSourceIndex(
            MashupSource mashup,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> topologicalOrder)
        {
            return topologicalOrder
                .Select((n, i) => ExpressionBuilder.GetVisualizerElement(n.Value) == mashup.Source ? (int?)i : null)
                .FirstOrDefault(index => index.HasValue);
        }

        public static DialogTypeVisualizer CreateVisualizer(
            this VisualizerFactory visualizerFactory,
            XElement visualizerSettings,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap)
        {
            DialogTypeVisualizer visualizer;
            if (visualizerSettings != null)
            {
                visualizerSettings.SetAttributeValue(XsdAttributeName, XsdAttributeValue);
                visualizerSettings.SetAttributeValue(XsiAttributeName, XsiAttributeValue);
                var serializer = new XmlSerializer(visualizerFactory.VisualizerType);
                using var reader = visualizerSettings.CreateReader();
                visualizer = (DialogTypeVisualizer)(serializer.CanDeserialize(reader)
                    ? serializer.Deserialize(reader)
                    : Activator.CreateInstance(visualizerFactory.VisualizerType));
            }
            else visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerFactory.VisualizerType);

            if (visualizer is MashupVisualizer mashupVisualizer)
            {
                int index = 0;
                var mashupSettings = visualizerSettings?.Elements(MashupSettingsElement) ?? Enumerable.Empty<XElement>();
                foreach (var mashup in mashupSettings)
                {
                    VisualizerFactory mashupFactory;
                    if (index < visualizerFactory.MashupSources.Count)
                    {
                        mashupFactory = visualizerFactory.MashupSources[index++];
                    }
                    else
                    {
                        var mashupSourceElement = mashup.Element(MashupSourceElement);
                        if (mashupSourceElement == null) continue;

                        var mashupSourceIndex = int.Parse(mashupSourceElement.Value);
                        var mashupSource = (InspectBuilder)visualizerLayout.DialogSettings[mashupSourceIndex]?.Tag;
                        var mashupVisualizerTypeName = mashup.Element(nameof(VisualizerDialogSettings.VisualizerTypeName))?.Value;
                        var mashupVisualizerType = typeVisualizerMap.GetVisualizerType(mashupVisualizerTypeName);
                        mashupFactory = new VisualizerFactory(mashupSource, mashupVisualizerType);
                    }

                    CreateMashupVisualizer(mashupVisualizer, visualizerFactory, mashupFactory, visualizerLayout, typeVisualizerMap, mashup);
                }

                for (int i = index; i < visualizerFactory.MashupSources.Count; i++)
                {
                    var mashupFactory = visualizerFactory.MashupSources[i];
                    CreateMashupVisualizer(mashupVisualizer, visualizerFactory, mashupFactory, visualizerLayout, typeVisualizerMap);
                }
            }

            return visualizer;
        }

        static void CreateMashupVisualizer(
            MashupVisualizer mashupVisualizer,
            VisualizerFactory visualizerFactory,
            VisualizerFactory mashupFactory,
            VisualizerLayout visualizerLayout,
            TypeVisualizerMap typeVisualizerMap,
            XElement mashup = null)
        {
            var mashupSourceType = GetMashupSourceType(
                visualizerFactory.VisualizerType,
                mashupFactory.VisualizerType,
                typeVisualizerMap);
            if (mashupSourceType == null) return;
            if (mashupSourceType != typeof(DialogTypeVisualizer))
            {
                mashupFactory = new VisualizerFactory(mashupFactory.Source, mashupSourceType);
            }

            var mashupVisualizerSettings = default(XElement);
            if (mashup != null)
            {
                var mashupVisualizerSettingsElement = mashup.Element(nameof(VisualizerDialogSettings.VisualizerSettings));
                mashupVisualizerSettings = mashupVisualizerSettingsElement.Elements().FirstOrDefault();
                if (mashup.Element(nameof(VisualizerDialogSettings.VisualizerTypeName)).Value != mashupFactory.VisualizerType.FullName)
                {
                    mashupVisualizerSettings = default;
                }
            }

            var nestedVisualizer = mashupFactory.CreateVisualizer(mashupVisualizerSettings, visualizerLayout, typeVisualizerMap);
            mashupVisualizer.MashupSources.Add(mashupFactory.Source, nestedVisualizer);
        }
    }
}
