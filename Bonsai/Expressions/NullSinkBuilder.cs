using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("NullSink", Namespace = Constants.XmlNamespace)]
    [Description("A null operator to allow branching out intermediate workflow values.")]
    public class NullSinkBuilder : ExpressionBuilder
    {
        public NullSinkBuilder()
            : base(minArguments: 1, maxArguments: 1)
        {
        }

        public override Expression Build()
        {
            return Arguments.Values.Single();
        }
    }
}
