using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    public abstract class SysexConfiguration
    {
        [Category("Data")]
        [Description("The name of the protocol extension configuration.")]
        public string Name { get; set; }

        public abstract void Configure(Arduino arduino);
    }
}
