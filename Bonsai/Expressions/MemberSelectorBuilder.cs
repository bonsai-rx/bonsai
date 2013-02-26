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
    public class MemberSelectorBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(observableType);
            var body = ExpressionHelper.MemberAccess(parameter, Selector);
            var selectorExpression = Expression.Lambda(body, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameter.Type, body.Type), Source, selectorExpression);
        }
    }
}
