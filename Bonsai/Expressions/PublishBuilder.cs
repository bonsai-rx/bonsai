using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;

namespace Bonsai.Expressions
{
    public class PublishBuilder : CombinatorBuilder
    {
        static readonly Type connectableObservableType = typeof(IConnectableObservable<>);
        static readonly MethodInfo publishMethod = typeof(Observable).GetMethods()
                                                                     .First(m => m.Name == "Publish" &&
                                                                            m.GetParameters().Length == 1);

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var publishVariable = Expression.Parameter(connectableObservableType.MakeGenericType(observableType));
            var publish = Expression.Assign(publishVariable, Expression.Call(publishMethod.MakeGenericMethod(observableType), Source));
            var connect = Expression.Call(publishVariable, "Connect", null);
            return Expression.Block(Enumerable.Repeat(publishVariable, 1), publish, connect, publishVariable);
        }
    }
}
