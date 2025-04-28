using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Editor.GraphModel;

namespace Bonsai.Editor.GraphView
{
    public sealed class WorkflowWatchSettings
    {
        [XmlElement("Watch")]
        public Collection<WorkflowElementWatchSettings> WatchList { get; } = new();
    }

    public sealed class WorkflowElementWatchSettings
    {
        [XmlIgnore]
        internal WorkflowEditorPath Path { get; set; }

        [XmlAttribute(nameof(Path))]
        public string PathXml
        {
            get => Path.ToString();
            set => Path = WorkflowEditorPath.Parse(value);
        }
    }
}
