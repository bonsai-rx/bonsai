using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    [XmlInclude(typeof(StepperConfiguration))]
    [Editor("Bonsai.Arduino.Design.SysexConfigurationCollectionEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
    public class SysexConfigurationCollection : Collection<SysexConfiguration>
    {
    }
}
