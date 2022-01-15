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
    /// <summary>
    /// Represents an operator that decodes the data contents from each OSC message in the sequence.
    /// </summary>
    [Description("Decodes the data contents from each OSC message in the sequence.")]
    public class Parse : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the name of the OSC method that should be matched
        /// against the address pattern in the OSC message.
        /// </summary>
        [Description("The name of the OSC method that should be matched against the address pattern in the OSC message.")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the OSC type tag specifying the type of the OSC
        /// arguments in the message.
        /// </summary>
        [TypeConverter(typeof(TypeTagConverter))]
        [Description("The OSC type tag specifying the type of the OSC arguments in the message.")]
        public string TypeTag { get; set; }

        /// <inheritdoc/>
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
                var typeTagArgument = Expression.Constant(typeTag);
                var readerParameter = Expression.Parameter(typeof(BinaryReader));
                var parseMessage = MessageParser.Content(typeTag, readerParameter);
                var messageParser = Expression.Lambda(parseMessage, readerParameter);
                return Expression.Call(typeof(Parse), nameof(Process), new[] { messageParser.ReturnType }, source, typeTagArgument, messageParser);
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

        static IObservable<TResult> Process<TResult>(IObservable<Message> source, string typeTag, Func<BinaryReader, TResult> messageParser)
        {
            return source.Select(message =>
            {
                const int CommaOffset = 1;
                if ((message.TypeTag.Length != typeTag.Length + CommaOffset) ||
                    string.Compare(message.TypeTag, CommaOffset, typeTag, 0, typeTag.Length) != 0)
                {
                    throw new InvalidOperationException($"Invalid message type tag: expected {typeTag}, actual {message.TypeTag.Substring(CommaOffset)}.");
                }

                var contents = message.GetContentStream();
                using (var reader = new BigEndianReader(contents))
                {
                    return messageParser(reader);
                }
            });
        }
    }
}
