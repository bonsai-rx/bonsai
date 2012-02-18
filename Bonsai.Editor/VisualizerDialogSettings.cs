using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Editor
{
    public class VisualizerDialogSettings
    {
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
    }
}
