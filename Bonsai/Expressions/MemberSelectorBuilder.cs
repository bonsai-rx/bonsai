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
    public class MemberSelectorBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Select" &&
                                                                           m.GetParameters().Length == 2);
        readonly Collection<string> selector = new Collection<string>();

        [TypeConverter("Bonsai.Design.MemberSelectorConverter, Bonsai.Design")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<string> Selector
        {
            get { return selector; }
        }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(observableType);
            Expression body = parameter;
            foreach (var memberName in selector)
            {
                body = Expression.PropertyOrField(body, memberName);
            }

            var selectorExpression = Expression.Lambda(body, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameter.Type, body.Type), Source, selectorExpression);
        }
    }
}
