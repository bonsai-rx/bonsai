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
                                                                       m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        public Type ObservableType { get; private set; }

        public IObservable<object> Output
        {
            get { return subject; }
        }

        public override Expression Build()
        {
            ObservableType = Source.Type.GetGenericArguments()[0];
            var actionType = Expression.GetActionType(ObservableType);

            var onNextMethod = subject.GetType().GetMethod("OnNext");
            var actionDelegate = Delegate.CreateDelegate(actionType, subject, onNextMethod);
            var action = Expression.Constant(actionDelegate);
            return Expression.Call(doMethod.MakeGenericMethod(ObservableType), Source, action);
        }
    }
}
