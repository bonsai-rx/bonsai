using Bonsai.Expressions;
using Bonsai.Osc.IO;
using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class SendMessage : CombinatorExpressionBuilder
    {
        public SendMessage()
            : base(minArguments: 1, maxArguments: 1)
        {
            Address = MessageBuilder.AddressSeparator;
        }

        [Editor("Bonsai.Osc.Design.TransportConfigurationEditor, Bonsai.Osc.Design", typeof(UITypeEditor))]
        public string Connection { get; set; }

        public string Address { get; set; }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterTypes = source.Type.GetGenericArguments();
            var inputParameter = Expression.Parameter(parameterTypes[0]);
            var writerParameter = Expression.Parameter(typeof(BigEndianWriter));
            var buildMessage = MessageBuilder.Message(Address, inputParameter, writerParameter);
            var messageBuilder = Expression.Lambda(buildMessage, inputParameter, writerParameter);
            var builder = Expression.Constant(this);
            return Expression.Call(builder, "Process", parameterTypes, source, messageBuilder);
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
