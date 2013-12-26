using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [TypeConverter("Bonsai.Design.ExpressionBuilderArgumentTypeConverter, Bonsai.Design")]
    public class ExpressionBuilderArgument
    {
        public const string Source = "Source";

        public ExpressionBuilderArgument()
            : this(Source + 1)
        {
        }

        public ExpressionBuilderArgument(string name)
        {
            Name = name;
        }

        [XmlText]
        public string Name { get; set; }
    }
}
