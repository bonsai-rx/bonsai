using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    public class VisualizerLayout
    {
        [XmlAttribute]
        public string Version { get; set; }

        [XmlElement(nameof(WindowSettings))]
        public VisualizerWindowSettingsCollection WindowSettings { get; } = new VisualizerWindowSettingsCollection();

        // [Obsolete]
        [XmlElement(nameof(DialogSettings))]
        public VisualizerWindowSettingsCollection DialogSettings { get; set; }

        public static VisualizerLayout Load(string uri)
        {
            using var reader = XmlReader.Create(uri);
            var layout = (VisualizerLayout)Serializer.Deserialize(reader);
            UpgradeLegacyLayout(layout);
            return layout;
        }

        private static void UpgradeLegacyLayout(VisualizerLayout layout)
        {
            if (layout?.DialogSettings is not null)
            {
                foreach (var legacySettings in layout.DialogSettings)
                {
                    UpgradeLegacyLayout(legacySettings.NestedLayout);
#pragma warning disable CS0612 // Type or member is obsolete
                    if (legacySettings is WorkflowEditorSettings editorSettings)
                    {
                        UpgradeLegacyLayout(editorSettings.EditorVisualizerLayout);
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    layout.WindowSettings.Add(legacySettings);
                }
                layout.DialogSettings = null;
            }
        }

        public static XmlSerializer Serializer
        {
            get { return SerializerFactory.instance; }
        }

        #region SerializerFactory

        static class SerializerFactory
        {
            internal static readonly XmlSerializer instance = new XmlSerializer(typeof(VisualizerLayout));
        }

        #endregion
    }
}
