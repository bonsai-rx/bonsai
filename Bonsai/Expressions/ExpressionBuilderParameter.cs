using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [TypeConverter("Bonsai.Design.ExpressionBuilderParameterTypeConverter, Bonsai.Design")]
    public class ExpressionBuilderParameter
    {
        public const string Source = "Source";

        public ExpressionBuilderParameter()
            : this(Source + 1)
        {
        }

        public ExpressionBuilderParameter(string value)
        {
            Value = value;
        }

        [XmlText]
        public string Value { get; set; }
    }
}
