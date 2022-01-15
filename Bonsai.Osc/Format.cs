using Bonsai.Expressions;
using Bonsai.Osc.IO;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an operator that formats each element of the sequence as
    /// an OSC message.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Formats each element of the sequence as an OSC message.")]
    public class Format : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the OSC address pattern specifying which method to invoke
        /// using the formatted data.
        /// </summary>
        [Description("The OSC address pattern specifying which method to invoke using the formatted data.")]
        public string Address { get; set; } = MessageBuilder.AddressSeparator;

        /// <inheritdoc/>
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
