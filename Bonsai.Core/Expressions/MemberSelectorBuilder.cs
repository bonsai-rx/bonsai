using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("MemberSelector", Namespace = Constants.XmlNamespace)]
    [Description("Selects inner properties of elements of the sequence.")]
    public class MemberSelectorBuilder : SelectBuilder
    {
        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

        protected override Expression BuildSelector(Expression expression)
        {
            var memberAccess = FindMemberAccess(Selector);
            return ExpressionHelper.MemberAccess(expression, memberAccess.Item2);
        }
    }
}
