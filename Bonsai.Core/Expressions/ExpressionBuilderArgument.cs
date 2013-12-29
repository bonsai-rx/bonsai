using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow argument assignment. This class determines the index of a
    /// workflow connection and is used to specify the order of input connections to
    /// any given node.
    /// </summary>
    [TypeConverter("Bonsai.Design.ExpressionBuilderArgumentTypeConverter, Bonsai.Design")]
    public class ExpressionBuilderArgument
    {
        /// <summary>
        /// The prefix that starts every input argument name.
        /// </summary>
        public const string ArgumentNamePrefix = "Source";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class.
        /// </summary>
        public ExpressionBuilderArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class with
        /// the specified argument index.
        /// </summary>
        /// <param name="index">The zero-based index of the input argument.</param>
        public ExpressionBuilderArgument(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class with
        /// the specified argument name.
        /// </summary>
        /// <param name="name">
        /// The name of the input argument. Arbitrary named arguments are not supported, so all
        /// names must start with the <see cref="ArgumentNamePrefix"/> followed by the one-based
        /// argument index.
        /// </param>
        public ExpressionBuilderArgument(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the zero-based index of the input argument.
        /// </summary>
        [XmlIgnore]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the input argument. Arbitrary named arguments are not supported, so all
        /// names must start with the <see cref="ArgumentNamePrefix"/> followed by the one-based
        /// argument index.
        /// </summary>
        [XmlText]
        public string Name
        {
            get { return ArgumentNamePrefix + (Index + 1); }
            set
            {
                if (!value.StartsWith(ArgumentNamePrefix))
                {
                    throw new ArgumentException(string.Format("Argument name prefix must start with '{0}'.", ArgumentNamePrefix), "value");
                }

                int index;
                var indexString = value.Substring(ArgumentNamePrefix.Length);
                if (string.IsNullOrEmpty(indexString)) index = 1;
                else if (!int.TryParse(indexString, out index))
                {
                    throw new ArgumentException("Argument name has an incorrect format.", "value");
                }

                Index = index - 1;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
