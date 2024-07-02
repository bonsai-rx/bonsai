using Bonsai.Dag;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        const string MashupSourceElement = "Source";

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

        public static void SetLayoutNotifications(ExpressionBuilderGraph source, VisualizerDialogMap lookup)
        {
            foreach (var builder in source.Descendants())
            {
                var inspectBuilder = (InspectBuilder)builder;
                if (lookup.TryGetValue((InspectBuilder)builder, out VisualizerDialogLauncher _))
                {
                    SetVisualizerNotifications(inspectBuilder);
                }
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
            VisualizerDialogSettings layoutSettings,
            TypeVisualizerMap typeVisualizerMap,
            ExpressionBuilderGraph workflow)
        {
            var inspectBuilder = ExpressionBuilder.GetVisualizerElement(source);
            if (inspectBuilder.ObservableType == null || !inspectBuilder.PublishNotifications ||
                source.Builder is VisualizerMappingBuilder)
            {
                return null;
            }

            var visualizerType = typeVisualizerMap.GetVisualizerType(layoutSettings?.VisualizerTypeName ?? string.Empty);
            visualizerType ??= typeVisualizerMap.GetTypeVisualizers(inspectBuilder).FirstOrDefault();
            if (visualizerType is null)
            {
                return null;
            }

            var mashupArguments = GetMashupArguments(inspectBuilder, typeVisualizerMap);
            var visualizerFactory = new VisualizerFactory(inspectBuilder, visualizerType, mashupArguments);
            var visualizer = new Lazy<DialogTypeVisualizer>(() => DeserializeVisualizerSettings(
                visualizerType,
                layoutSettings,
                workflow,
                visualizerFactory,
                typeVisualizerMap));

            var launcher = new VisualizerDialogLauncher(visualizer, visualizerFactory, workflow, source);
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
            ExpressionBuilderGraph workflow,
            VisualizerFactory visualizerFactory,
            TypeVisualizerMap typeVisualizerMap)
        {
            if (layoutSettings?.VisualizerTypeName != visualizerType?.FullName)
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

            return visualizerFactory.CreateVisualizer(layoutSettings?.VisualizerSettings, workflow, typeVisualizerMap);
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
            ExpressionBuilderGraph workflow,
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
                        var mashupSource = (InspectBuilder)workflow[mashupSourceIndex].Value;
                        var mashupVisualizerTypeName = mashup.Element(nameof(VisualizerDialogSettings.VisualizerTypeName))?.Value;
                        var mashupVisualizerType = typeVisualizerMap.GetVisualizerType(mashupVisualizerTypeName);
                        mashupFactory = new VisualizerFactory(mashupSource, mashupVisualizerType);
                    }

                    CreateMashupVisualizer(mashupVisualizer, visualizerFactory, mashupFactory, workflow, typeVisualizerMap, mashup);
                }

                for (int i = index; i < visualizerFactory.MashupSources.Count; i++)
                {
                    var mashupFactory = visualizerFactory.MashupSources[i];
                    CreateMashupVisualizer(mashupVisualizer, visualizerFactory, mashupFactory, workflow, typeVisualizerMap);
                }
            }

            return visualizer;
        }

        static void CreateMashupVisualizer(
            MashupVisualizer mashupVisualizer,
            VisualizerFactory visualizerFactory,
            VisualizerFactory mashupFactory,
            ExpressionBuilderGraph workflow,
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

            var nestedVisualizer = mashupFactory.CreateVisualizer(mashupVisualizerSettings, workflow, typeVisualizerMap);
            mashupVisualizer.MashupSources.Add(mashupFactory.Source, nestedVisualizer);
        }
    }
}
