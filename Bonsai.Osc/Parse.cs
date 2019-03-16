using Bonsai.Expressions;
using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    [Description("Retrieves data contents from an OSC message.")]
    public class Parse : SelectBuilder
    {
        [TypeConverter(typeof(TypeTagConverter))]
        [Description("The OSC type tag specifying the contents of the message.")]
        public string TypeTag { get; set; }

        protected override Expression BuildSelector(Expression expression)
        {
            var readerParameter = Expression.Parameter(typeof(BinaryReader));
            var builder = Expression.Constant(this);
            if (string.IsNullOrEmpty(TypeTag))
            {
                return expression;
            }
            else
            {
                var parseMessage = MessageParser.Content(TypeTag, readerParameter);
                var messageParser = Expression.Lambda(parseMessage, readerParameter);
                return Expression.Call(builder, "Process", new[] { messageParser.ReturnType }, expression, messageParser);
            }
        }

        TResult Process<TResult>(Message message, Func<BinaryReader, TResult> messageReader)
        {
            var contents = message.GetStream();
            using (var reader = new BigEndianReader(contents))
            {
                return messageReader(reader);
            }
        }
    }
}
