using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Bonsai.Osc
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Reads data from an Open Sound Control communication channel.")]
    public class ReceiveMessage : Parse
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

        public ReceiveMessage()
        {
            Address = MessageBuilder.AddressSeparator;
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        [TypeConverter(typeof(ConnectionNameConverter))]
        [Description("The communication channel to use for the OSC protocol.")]
        public string Connection { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = Expression.Parameter(typeof(IObservable<Message>));
            var parseMessage = Build(source);
            var processor = Expression.Lambda(parseMessage, source);
            var connection = Expression.Constant(Connection);
            var resultType = processor.ReturnType.GetGenericArguments();
            return Expression.Call(typeof(ReceiveMessage), nameof(Generate), resultType, connection, processor);
        }

        static IObservable<TResult> Generate<TResult>(string connection, Func<IObservable<Message>, IObservable<TResult>> processor)
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(connection),
                connection => processor(connection.Transport.MessageReceived));
        }
    }
}
