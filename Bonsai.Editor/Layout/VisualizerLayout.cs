using System.Xml.Serialization;

namespace Bonsai.Design
{
    public class VisualizerLayout
    {
        [XmlElement(nameof(DialogSettings))]
        public VisualizerDialogSettingsCollection DialogSettings { get; } = new VisualizerDialogSettingsCollection();

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
