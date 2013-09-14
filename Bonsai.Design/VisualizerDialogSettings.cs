using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Bonsai.Design
{
    [XmlInclude(typeof(WorkflowEditorSettings))]
    public class VisualizerDialogSettings
    {
        readonly Collection<int> mashups = new Collection<int>();

        [XmlIgnore]
        public object Tag { get; set; }

        public bool Visible { get; set; }

        public Point Location { get; set; }

        public Size Size { get; set; }

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

        public Collection<int> Mashups
        {
            get { return mashups; }
        }

        public XElement VisualizerSettings { get; set; }
    }
}
