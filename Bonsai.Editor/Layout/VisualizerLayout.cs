using System.Xml.Serialization;

namespace Bonsai.Design
{
    public class VisualizerLayout
    {
        readonly VisualizerDialogSettingsCollection dialogSettings = new VisualizerDialogSettingsCollection();

        [XmlElement("DialogSettings")]
        public VisualizerDialogSettingsCollection DialogSettings
        {
            get { return dialogSettings; }
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
