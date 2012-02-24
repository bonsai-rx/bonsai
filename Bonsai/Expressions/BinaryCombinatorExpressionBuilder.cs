using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public abstract class BinaryCombinatorExpressionBuilder : CombinatorExpressionBuilder
    {
        [XmlIgnore]
        [Browsable(false)]
        public Expression Other { get; set; }
    }
}
