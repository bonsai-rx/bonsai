using System.Drawing;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    [XmlInclude(typeof(WorkflowEditorSettings))]
    public class VisualizerDialogSettings
    {
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

        // [Obsolete]
        public Collection<int> Mashups { get; } = new Collection<int>();

        public bool MashupsSpecified
        {
            get { return false; }
        }
    }
}
