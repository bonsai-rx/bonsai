using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Editor
{
    public class VisualizerLayout
    {
        readonly VisualizerDialogSettingsCollection dialogSettings = new VisualizerDialogSettingsCollection();

        [XmlElement("DialogSettings")]
        public VisualizerDialogSettingsCollection DialogSettings
        {
            get { return dialogSettings; }
        }
    }
}
