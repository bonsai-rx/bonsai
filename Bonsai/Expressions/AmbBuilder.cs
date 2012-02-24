using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("Amb", Namespace = Constants.XmlNamespace)]
    public class AmbBuilder : BinaryCombinatorExpressionBuilder
    {
        static readonly MethodInfo ambMethod = typeof(Observable).GetMethods().First(m => m.Name == "Amb" &&
                                                                                     m.GetParameters().Length == 2);

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            return Expression.Call(ambMethod.MakeGenericMethod(sourceType), Source, Other);
        }
    }
}
