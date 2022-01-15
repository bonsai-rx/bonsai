using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that reads data contents from the specified OSC
    /// communication channel.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Reads data contents from the specified OSC communication channel.")]
    public class ReceiveMessage : Parse
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveMessage"/> class.
        /// </summary>
        public ReceiveMessage()
        {
            Address = MessageBuilder.AddressSeparator;
        }

        /// <inheritdoc/>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        /// <summary>
        /// Gets or sets the name of the communication channel to reserve
        /// for the OSC protocol.
        /// </summary>
        [TypeConverter(typeof(ConnectionNameConverter))]
        [Description("The name of the communication channel to reserve for the OSC protocol.")]
        public string Connection { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = Expression.Parameter(typeof(IObservable<Message>));
            var parseMessage = Build(source);
            var processor = Expression.Lambda(parseMessage, source);
            var connection = Expression.Constant(Connection, typeof(string));
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
