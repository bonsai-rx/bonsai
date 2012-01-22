using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Do")]
    public class DoBuilder : CombinatorBuilder
    {
        static readonly MethodInfo doMethod = typeof(Observable).GetMethods()
                                                                .First(m => m.Name == "Do" &&
                                                                       m.GetParameters().Length == 2 &&
                                                                       m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        [Browsable(false)]
        public LoadableElement Sink { get; set; }

        public override Expression Build()
        {
            Delegate actionDelegate;
            var observableType = Source.Type.GetGenericArguments()[0];

            var dynamicSink = Sink as DynamicSink;
            if (dynamicSink != null)
            {
                var createMethod = dynamicSink.GetType().GetMethod("Create");
                createMethod = createMethod.MakeGenericMethod(observableType);
                actionDelegate = (Delegate)createMethod.Invoke(dynamicSink, null);
            }
            else
            {
                var actionType = Expression.GetActionType(observableType);
                var processMethod = Sink.GetType().GetMethod("Process");
                actionDelegate = Delegate.CreateDelegate(actionType, Sink, processMethod);
            }

            var action = Expression.Constant(actionDelegate);
            return Expression.Call(doMethod.MakeGenericMethod(observableType), Source, action);
        }
    }
}
