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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Formats input data as an Open Sound Control message.")]
    public class Format : SelectBuilder
    {
        public Format()
        {
            Address = MessageBuilder.AddressSeparator;
        }

        [Description("The OSC address on which to broadcast the input data.")]
        public string Address { get; set; }

        protected override Expression BuildSelector(Expression expression)
        {
            var address = Address;
            if (expression.Type == typeof(Message) && string.IsNullOrEmpty(address))
            {
                return expression;
            }

            var inputParameter = Expression.Parameter(expression.Type);
            var writerParameter = Expression.Parameter(typeof(BigEndianWriter));
            var buildMessage = MessageBuilder.Message(address, inputParameter, writerParameter);
            var messageBuilder = Expression.Lambda(buildMessage, inputParameter, writerParameter);
            return Expression.Call(typeof(Format), nameof(BuildMessage), new[] { expression.Type }, expression, messageBuilder);
        }

        static Message BuildMessage<TSource>(TSource source, Action<TSource, BigEndianWriter> messageBuilder)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BigEndianWriter(stream))
            {
                messageBuilder(source, writer);
                var buffer = stream.ToArray();
                return new Message(buffer);
            }
        }
    }
}
