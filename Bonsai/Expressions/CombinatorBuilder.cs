using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public abstract class CombinatorBuilder : ExpressionBuilder
    {
        [XmlIgnore]
        [Browsable(false)]
        public Expression Source { get; set; }
    }
}
