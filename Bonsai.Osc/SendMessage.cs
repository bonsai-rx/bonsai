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
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Writes input data to an Open Sound Control communication channel.")]
    public class SendMessage : SingleArgumentExpressionBuilder
    {
        public SendMessage()
        {
            Address = MessageBuilder.AddressSeparator;
        }

        [TypeConverter(typeof(ConnectionNameConverter))]
        [Description("The communication channel to use for the OSC protocol.")]
        public string Connection { get; set; }

        [Description("The OSC address on which to broadcast the input data.")]
        public string Address { get; set; }

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
