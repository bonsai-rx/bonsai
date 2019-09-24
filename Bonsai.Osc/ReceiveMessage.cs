using Bonsai.Expressions;
using Bonsai.Osc.IO;
using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Reads data from an Open Sound Control communication channel.")]
    public class ReceiveMessage : CombinatorExpressionBuilder
    {
        public ReceiveMessage()
            : base(minArguments: 0, maxArguments: 0)
        {
            Address = MessageBuilder.AddressSeparator;
        }

        [TypeConverter(typeof(ConnectionNameConverter))]
        [Description("The communication channel to use for the OSC protocol.")]
        public string Connection { get; set; }

        [Description("The OSC address space on which the received data is being broadcast.")]
        public string Address { get; set; }

        [TypeConverter(typeof(TypeTagConverter))]
        [Description("The OSC type tag specifying the contents of the message.")]
        public string TypeTag { get; set; }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            var readerParameter = Expression.Parameter(typeof(BinaryReader));
            var builder = Expression.Constant(this);

            if (string.IsNullOrEmpty(TypeTag))
            {
                return Expression.Call(builder, "Generate", null);
            }
            else
            {
                var parseMessage = MessageParser.Content(TypeTag, readerParameter);
                var messageParser = Expression.Lambda(parseMessage, readerParameter);
                return Expression.Call(builder, "Generate", new[] { messageParser.ReturnType }, messageParser);
            }
        }

        IObservable<Message> Generate()
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(Connection),
                connection => connection.Transport.MessageReceived
                    .Where(message =>
                    {
                        var address = Address;
                        return string.IsNullOrEmpty(address) || message.IsMatch(address);
                    }))
                    .SubscribeOn(TaskPoolScheduler.Default);
        }

        IObservable<TSource> Generate<TSource>(Func<BinaryReader, TSource> messageReader)
        {
            return Generate().Select(message =>
            {
                var contents = message.GetStream();
                using (var reader = new BigEndianReader(contents))
                {
                    return messageReader(reader);
                }
            });
        }
    }
}
