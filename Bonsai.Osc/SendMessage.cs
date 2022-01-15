using Bonsai.Expressions;
using Bonsai.Osc.IO;
using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that writes each element of the sequence into an
    /// OSC communication channel.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Writes each element of the sequence into an OSC communication channel.")]
    public class SendMessage : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the name of the communication channel to reserve
        /// for the OSC protocol.
        /// </summary>
        [TypeConverter(typeof(ConnectionNameConverter))]
        [Description("The name of the communication channel to reserve for the OSC protocol.")]
        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets the OSC address pattern specifying which method to invoke
        /// using the formatted data.
        /// </summary>
        [Description("The OSC address pattern specifying which method to invoke using the formatted data.")]
        public string Address { get; set; } = MessageBuilder.AddressSeparator;

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var address = Address;
            var source = arguments.First();
            var parameterTypes = source.Type.GetGenericArguments();
            var inputParameter = Expression.Parameter(parameterTypes[0]);
            var builder = Expression.Constant(this);
            if (inputParameter.Type == typeof(Message) && string.IsNullOrEmpty(address))
            {
                return Expression.Call(builder, nameof(Process), null, source);
            }

            var writerParameter = Expression.Parameter(typeof(BigEndianWriter));
            var buildMessage = MessageBuilder.Message(address, inputParameter, writerParameter);
            var messageBuilder = Expression.Lambda(buildMessage, inputParameter, writerParameter);
            return Expression.Call(builder, nameof(Process), parameterTypes, source, messageBuilder);
        }

        IObservable<Message> Process(IObservable<Message> source)
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(Connection),
                connection => source.Do(input =>
                {
                    connection.Transport.SendPacket(writer => writer.Write(
                        input.Buffer.Array,
                        input.Buffer.Offset,
                        input.Buffer.Count));
                }));
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, Action<TSource, BigEndianWriter> messageBuilder)
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(Connection),
                connection => source.Do(input =>
                {
                    connection.Transport.SendPacket(writer => messageBuilder(input, writer));
                }));
        }
    }
}
