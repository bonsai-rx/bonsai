using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("SkipUntil", Namespace = Constants.XmlNamespace)]
    public class SkipUntilBuilder : BinaryCombinatorBuilder
    {
        static readonly MethodInfo skipUntilMethod = typeof(Observable).GetMethod("SkipUntil");

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            return Expression.Call(skipUntilMethod.MakeGenericMethod(sourceType, otherType), Source, Other);
        }
    }
}
