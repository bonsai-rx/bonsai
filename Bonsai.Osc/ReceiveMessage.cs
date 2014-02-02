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
    [WorkflowElementCategory(ElementCategory.Source)]
    public class ReceiveMessage : CombinatorExpressionBuilder
    {
        public ReceiveMessage()
            : base(minArguments: 0, maxArguments: 0)
        {
            Address = MessageBuilder.AddressSeparator;
        }

        [Editor("Bonsai.Osc.Design.TransportConfigurationEditor, Bonsai.Osc.Design", typeof(UITypeEditor))]
        public string Connection { get; set; }

        public string Address { get; set; }

        public string TypeTag { get; set; }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            var addressReaderParameter = Expression.Parameter(typeof(BigEndianReader));
            var readerParameter = Expression.Parameter(typeof(BigEndianReader));
            var parseAddress = MessageParser.Address(addressReaderParameter);
            var addressParser = Expression.Lambda(parseAddress, addressReaderParameter);

            var parseMessage = MessageParser.Content(TypeTag, readerParameter);
            var messageParser = Expression.Lambda(parseMessage, readerParameter);
            var builder = Expression.Constant(this);
            return Expression.Call(builder, "Generate", new[] { messageParser.ReturnType }, addressParser, messageParser);
        }

        IObservable<TSource> Generate<TSource>(Func<BigEndianReader, string> addressReader, Func<BigEndianReader, TSource> messageReader)
        {
            return Observable.Using(
                () => TransportManager.ReserveConnection(Connection),
                connection => connection.Transport.MessageReceived
                    .Where(message => message.IsMatch(Address))
                    .Select(message =>
                {
                    var contents = message.GetContents();
                    return messageReader(contents);
                }));
        }
    }
}
