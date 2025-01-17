using System.Drawing;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
#pragma warning disable CS0612 // Type or member is obsolete
    [XmlInclude(typeof(WorkflowEditorSettings))]
#pragma warning restore CS0612 // Type or member is obsolete
    public class VisualizerDialogSettings
    {
        [XmlIgnore]
        public int? Index { get; set; }

        [XmlAttribute(nameof(Index))]
        public string IndexXml
        {
            get => Index.HasValue ? Index.GetValueOrDefault().ToString() : null;
            set => Index = !string.IsNullOrEmpty(value) ? int.Parse(value) : null;
        }

        [XmlIgnore]
        public object Tag { get; set; }

        public bool Visible { get; set; }

        public Point Location { get; set; }

        public Size Size { get; set; }

        public FormWindowState WindowState { get; set; }

        [XmlIgnore]
        public Rectangle Bounds
        {
            get { return new Rectangle(Location, Size); }
            set
            {
                Location = value.Location;
                Size = value.Size;
            }
        }

        public string VisualizerTypeName { get; set; }

        public XElement VisualizerSettings { get; set; }

        public VisualizerLayout NestedLayout { get; set; }

        // [Obsolete]
        public Collection<int> Mashups { get; } = new Collection<int>();

        public bool VisibleSpecified => Visible;

        public bool LocationSpecified => !Location.IsEmpty;

        public bool SizeSpecified => !Size.IsEmpty;

        public bool WindowStateSpecified => WindowState != FormWindowState.Normal;

        public bool NestedLayoutSpecified => NestedLayout?.DialogSettings.Count > 0;

        public bool MashupsSpecified => false;
    }
}
