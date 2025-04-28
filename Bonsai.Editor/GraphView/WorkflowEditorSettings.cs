using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bonsai.Editor.GraphView
{
    [XmlRoot("EditorSettings")]
    public sealed class WorkflowEditorSettings
    {
        [XmlAttribute]
        public string Version { get; set; }

        [XmlAnyElement]
        public XElement DockPanel { get; set; }

        public WorkflowWatchSettings WatchSettings { get; set; }

        public static XmlSerializer Serializer
        {
            get { return SerializerFactory.instance; }
        }

        #region SerializerFactory

        static class SerializerFactory
        {
            internal static readonly XmlSerializer instance = new(typeof(WorkflowEditorSettings));
        }

        #endregion
    }
}
