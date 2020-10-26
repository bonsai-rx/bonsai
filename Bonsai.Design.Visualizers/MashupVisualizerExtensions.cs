using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    static class MashupVisualizerExtensions
    {
        public static XElement[] Serialize(this IReadOnlyList<VisualizerMashup> mashups)
        {
            return mashups.Select(mashup => SerializeVisualizer((MashupVisualizer)mashup.Visualizer)).ToArray();
        }

        public static void Deserialize(this IReadOnlyList<VisualizerMashup> mashups, XElement[] xml)
        {
            if (xml != null)
            {
                for (int i = 0; i < xml.Length && i < mashups.Count; i++)
                {
                    DeserializeVisualizer(xml[i], (MashupVisualizer)mashups[i].Visualizer);
                }
            }
        }

        static XElement SerializeVisualizer(MashupVisualizer visualizer)
        {
            var document = new XDocument();
            var serializer = new XmlSerializer(visualizer.GetType());
            using (var writer = document.CreateWriter())
            {
                serializer.Serialize(writer, visualizer);
            }
            document.Root.AddFirst(new XElement("VisualizerTypeName", visualizer.VisualizerType.FullName));
            return document.Root;
        }

        static void DeserializeVisualizer(XElement root, MashupVisualizer visualizer)
        {
            var visualizerTypeName = root.Element("VisualizerTypeName").Value;
            if (visualizerTypeName == visualizer.VisualizerType.FullName)
            {
                var visualizerType = visualizer.GetType();
                var visualizerProperty = visualizerType.GetProperty(nameof(MashupVisualizer<TableLayoutPanelVisualizer>.Visualizer));
                if (visualizerProperty == null)
                {
                    throw new InvalidOperationException("Incompatible mashup visualizer object.");
                }

                var serializer = new XmlSerializer(visualizerType);
                using (var reader = root.CreateReader())
                {
                    var mashupSettings = (MashupVisualizer)serializer.Deserialize(reader);
                    var visualizerSettings = visualizerProperty.GetValue(mashupSettings);
                    visualizerProperty.SetValue(visualizer, visualizerSettings);
                }
            }
        }
    }
}
