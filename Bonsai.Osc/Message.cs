using Bonsai.Osc.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Bonsai.Osc
{
    /// <summary>
    /// Represents an OSC message, including its method address and data arguments.
    /// </summary>
    public sealed class Message
    {
        const char AddressSeparator = '/';
        readonly int contentSize;
        readonly int contentOffset;
        readonly MessagePattern[] addressParts;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class using the
        /// specified binary payload.
        /// </summary>
        /// <param name="array">
        /// An 8-bit unsigned integer array representing the contents of an OSC message.
        /// </param>
        public Message(byte[] array)
            : this(array, 0, array.Length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class using the
        /// specified array segment.
        /// </summary>
        /// <param name="buffer">
        /// A section of an 8-bit unsigned integer array representing the contents
        /// of an OSC message.
        /// </param>
        public Message(ArraySegment<byte> buffer)
            : this(buffer.Array, buffer.Offset, buffer.Count)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class using the
        /// specified array segment.
        /// </summary>
        /// <param name="array">
        /// An 8-bit unsigned integer array storing the contents of the OSC message.
        /// </param>
        /// <param name="offset">
        /// The zero-based starting position of the OSC message in the array.
        /// </param>
        /// <param name="count">
        /// The number of bytes used by the contents of the OSC message.
        /// </param>
        public Message(byte[] array, int offset, int count)
        {
            contentOffset = offset;
            Address = Dispatcher.ReadString(array, ref contentOffset);
            TypeTag = Dispatcher.ReadString(array, ref contentOffset);
            Buffer = new ArraySegment<byte>(array, offset, count);
            contentSize = count - (contentOffset - offset);
            addressParts = Array.ConvertAll(
                Address.Split(AddressSeparator),
                pattern => new MessagePattern(pattern));
        }

        /// <summary>
        /// Gets the OSC address pattern specifying which method to invoke
        /// with the data in this message.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Gets or sets the OSC type tag specifying the type of the OSC
        /// arguments in the message.
        /// </summary>
        public string TypeTag { get; private set; }

        /// <summary>
        /// Gets the an 8-bit unsigned integer array segment representing the contents
        /// of this OSC message.
        /// </summary>
        public ArraySegment<byte> Buffer { get; }

        /// <summary>
        /// Returns whether the OSC address pattern in this message matches the
        /// specified method name.
        /// </summary>
        /// <param name="methodName">
        /// The name of the OSC method to match against the OSC address pattern of
        /// this message.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the method matches the OSC address pattern;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsMatch(string methodName)
        {
            var parts = methodName.Split(AddressSeparator);
            if (addressParts.Length == parts.Length)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    var methodPart = parts[i];
                    var addressPart = addressParts[i];
                    if (!addressPart.IsMatch(methodPart))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens a memory stream into the OSC message contents.
        /// </summary>
        /// <returns>
        /// An instance of the <see cref="Stream"/> class which can be used
        /// to read the binary contents of the OSC message.
        /// </returns>
        public Stream GetContentStream()
        {
            return new MemoryStream(Buffer.Array, contentOffset, contentSize, false);
        }

        /// <summary>
        /// Returns an enumerable sequence of the OSC arguments in this message.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable"/> object which can be used to enumerate
        /// all the OSC arguments in this message.
        /// </returns>
        public IEnumerable GetContents()
        {
            var chars = TypeTag.ToArray();
            var activeArrays = new Stack<ArrayList>();
            using (var reader = new BigEndianReader(GetContentStream()))
            {
                for (int i = 1; i < chars.Length; i++)
                {
                    object content;
                    switch (chars[i])
                    {
                        case Osc.TypeTag.Int32:
                            content = reader.ReadInt32();
                            break;
                        case Osc.TypeTag.Float:
                            content = reader.ReadSingle();
                            break;
                        case Osc.TypeTag.String:
                        case Osc.TypeTag.Alternate:
                            content = MessageParser.ReadString(reader);
                            break;
                        case Osc.TypeTag.Blob:
                            content = MessageParser.ReadBlob(reader);
                            break;
                        case Osc.TypeTag.Int64:
                            content = reader.ReadInt64();
                            break;
                        case Osc.TypeTag.TimeTag:
                            content = MessageParser.ReadTimeTag(reader);
                            break;
                        case Osc.TypeTag.Double:
                            content = reader.ReadDouble();
                            break;
                        case Osc.TypeTag.Char:
                            content = MessageParser.ReadChar(reader);
                            break;
                        case Osc.TypeTag.True:
                            content = true;
                            break;
                        case Osc.TypeTag.False:
                            content = false;
                            break;
                        case Osc.TypeTag.Infinitum:
                            content = float.PositiveInfinity;
                            break;
                        case Osc.TypeTag.ArrayBegin:
                            activeArrays.Push(new ArrayList());
                            continue;
                        case Osc.TypeTag.ArrayEnd:
                            var array = activeArrays.Pop();
                            content = array.ToArray();
                            break;
                        default:
                        case Osc.TypeTag.Nil:
                            content = null;
                            break;
                    }

                    if (activeArrays.Count > 0)
                    {
                        var array = activeArrays.Peek();
                        array.Add(content);
                    }
                    else yield return content;
                }
            }
        }

        static string ToString(object obj)
        {
            if (obj is object[] array)
            {
                var values = new string[array.Length + 2];
                values[0] = "[";
                values[values.Length - 1] = "]";
                for (int i = 0; i < array.Length; i++)
                {
                    values[i + 1] = ToString(array[i]);
                }
                return string.Join(CultureInfo.InvariantCulture.TextInfo.ListSeparator, values);
            }
            else if (obj is byte[] blob) return string.Format("{0}[{1}]", typeof(byte).Name, blob.Length);
            else return obj.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var contentValues = GetContents().Cast<object>().Select(obj => ToString(obj)).ToArray();
            var contents = string.Join(CultureInfo.InvariantCulture.TextInfo.ListSeparator, contentValues);
            return string.Format("Address: {0} TypeTag: {1} Contents: {{{2}}}", Address, TypeTag, contents);
        }
    }
}
