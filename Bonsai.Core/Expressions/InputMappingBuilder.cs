using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("InputMapping", Namespace = Constants.XmlNamespace)]
    public class InputMappingBuilder : PropertyMappingBuilder
    {
        readonly MemberSelectorBuilder selector = new MemberSelectorBuilder();

        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector
        {
            get { return selector.Selector; }
            set { selector.Selector = value; }
        }

        internal override bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            base.BuildArgument(source, successor, out argument);
            argument = selector.Build(argument);
            return true;
        }
    }
}
