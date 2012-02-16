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
    public class InspectBuilder : CombinatorBuilder
    {
        Subject<object> subject = new Subject<object>();
        static readonly MethodInfo doMethod = typeof(Observable).GetMethods()
                                                                .First(m => m.Name == "Do" &&
                                                                       m.GetParameters().Length == 2 &&
                                                                       m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                       m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        public Type ObservableType { get; private set; }

        public IObservable<object> Output
        {
            get { return subject; }
        }

        public override Expression Build()
        {
            ObservableType = Source.Type.GetGenericArguments()[0];
            var subjectInstance = Expression.Constant(subject);

            var actionParameter = Expression.Parameter(ObservableType);
            var actionBody = Expression.Call(subjectInstance, "OnNext", null, Expression.Convert(actionParameter, typeof(object)));
            var action = Expression.Lambda(actionBody, actionParameter);
            return Expression.Call(doMethod.MakeGenericMethod(ObservableType), Source, action);
        }
    }
}
