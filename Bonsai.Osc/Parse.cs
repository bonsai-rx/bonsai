using Bonsai.Expressions;
using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Osc
{
    [Description("Decodes data contents from an OSC message stream.")]
    public class Parse : SingleArgumentExpressionBuilder
    {
        [Description("The OSC address space on which the received data is being broadcast.")]
        public string Address { get; set; }

        [TypeConverter(typeof(TypeTagConverter))]
        [Description("The OSC type tag specifying the contents of the message.")]
        public string TypeTag { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            return Build(source);
        }

        internal Expression Build(Expression source)
        {
            var parameterType = source.Type.GetGenericArguments()[0];
            if (parameterType != typeof(Message))
            {
                source = Expression.Call(typeof(Parse), nameof(Convert), null, source);
            }

            var address = Address;
            if (!string.IsNullOrEmpty(address))
            {
                source = Expression.Call(typeof(Parse), nameof(Filter), null, source, Expression.Constant(address));
            }

            var typeTag = TypeTag;
            if (string.IsNullOrEmpty(typeTag)) return source;
            else
            {
                var readerParameter = Expression.Parameter(typeof(BinaryReader));
                var parseMessage = MessageParser.Content(typeTag, readerParameter);
                var messageParser = Expression.Lambda(parseMessage, readerParameter);
                return Expression.Call(typeof(Parse), nameof(Process), new[] { messageParser.ReturnType }, source, messageParser);
            }
        }

        static IObservable<Message> Convert(IObservable<byte[]> source)
        {
            return Convert(source.Select(array => new ArraySegment<byte>(array)));
        }

        static IObservable<Message> Convert(IObservable<ArraySegment<byte>> source)
        {
            return Observable.Create<Message>(observer =>
            {
                var dispatcher = new Dispatcher(observer, HighResolutionScheduler.Default);
                var messageObserver = Observer.Create<ArraySegment<byte>>(
                    buffer => dispatcher.Process(buffer),
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(messageObserver);
            });
        }

        static IObservable<Message> Filter(IObservable<Message> source, string address)
        {
            return source.Where(message => message.IsMatch(address));
        }

        static IObservable<TResult> Process<TResult>(IObservable<Message> source, Func<BinaryReader, TResult> messageReader)
        {
            return source.Select(message =>
            {
                var contents = message.GetContentStream();
                using (var reader = new BigEndianReader(contents))
                {
                    return messageReader(reader);
                }
            });
        }
    }
}
