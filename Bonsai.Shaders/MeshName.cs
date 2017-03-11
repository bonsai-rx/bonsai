﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    public class MeshName
    {
        [XmlText]
        [Description("The name of the mesh geometry to aggregate.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string Name { get; set; }

        [DefaultValue(0)]
        [XmlAttribute("divisor")]
        [Description("Optionally specifies the number of instances populated by each buffer item in case of instanced rendering.")]
        public int Divisor { get; set; }
    }
}
