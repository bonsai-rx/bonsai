using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public class PublishBuilder : CombinatorExpressionBuilder
    {
        static readonly Type connectableObservableType = typeof(IConnectableObservable<>);
        static readonly ConstructorInfo connectorConstructor = typeof(ConnectableElement).GetConstructors().First();
        static readonly MethodInfo publishMethod = typeof(Observable).GetMethods()
                                                                     .First(m => m.Name == "Publish" &&
                                                                            m.GetParameters().Length == 1);

        [XmlIgnore]
        [Browsable(false)]
        public ConnectableElement Connector { get; set; }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var publishVariable = Expression.Parameter(connectableObservableType.MakeGenericType(observableType));
            var publish = Expression.Assign(publishVariable, Expression.Call(publishMethod.MakeGenericMethod(observableType), Source));
            var connect = Expression.Call(publishVariable, "Connect", null);

            var connector = Expression.Property(Expression.Constant(this), "Connector");
            var assignConnector = Expression.Assign(connector, Expression.New(connectorConstructor, Expression.Lambda(connect)));
            return Expression.Block(Enumerable.Repeat(publishVariable, 1), publish, assignConnector, publishVariable);
        }
    }
}
