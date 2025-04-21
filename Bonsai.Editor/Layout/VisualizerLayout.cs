using System.Xml.Serialization;

namespace Bonsai.Design
{
    public class VisualizerLayout
    {
        [XmlElement(nameof(WindowSettings))]
        public VisualizerWindowSettingsCollection WindowSettings { get; } = new VisualizerWindowSettingsCollection();

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
