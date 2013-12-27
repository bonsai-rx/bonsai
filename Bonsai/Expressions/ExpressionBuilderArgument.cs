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
        {
        }

        public ExpressionBuilderArgument(int index)
        {
            Index = index;
        }

        public ExpressionBuilderArgument(string name)
        {
            Name = name;
        }

        [XmlIgnore]
        public int Index { get; set; }

        [XmlText]
        public string Name
        {
            get { return Source + (Index + 1); }
            set
            {
                if (!value.StartsWith(Source))
                {
                    throw new ArgumentException(string.Format("Argument name prefix must start with '{0}'.", Source), "value");
                }

                int index;
                var indexString = value.Substring(Source.Length);
                if (string.IsNullOrEmpty(indexString)) index = 1;
                else if (!int.TryParse(indexString, out index))
                {
                    throw new ArgumentException("Argument name has an incorrect format.", "value");
                }

                Index = index - 1;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
