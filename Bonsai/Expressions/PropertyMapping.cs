using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    public sealed class PropertyMapping
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("selector")]
        public string Selector { get; set; }
    }
}
