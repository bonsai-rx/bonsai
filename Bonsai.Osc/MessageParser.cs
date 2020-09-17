using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bonsai.Osc
{
    static class MessageParser
    {
        const int PadLength = 4;
        static readonly MethodInfo ReadBytes = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadBytes));
        static readonly MethodInfo ReadInt32 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt32));
        static readonly MethodInfo ReadInt64 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt64));
        static readonly MethodInfo ReadUInt64 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadUInt64));
        static readonly MethodInfo ReadFloat = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadSingle));
        static readonly MethodInfo ReadDouble = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadDouble));

        internal static DateTimeOffset ReadTimeTag(BinaryReader reader)
        {
            var timeTag = reader.ReadUInt64();
            return TimeTag.ToTimestamp(timeTag);
        }

        internal static byte[] ReadBlob(BinaryReader reader)
        {
            var blobSize = reader.ReadInt32();
            return ReadBlob(reader, blobSize);
        }

        internal static byte[] ReadBlob(BinaryReader reader, int blobSize)
        {
            var bytes = reader.ReadBytes(blobSize);
            var zeroBytes = blobSize % PadLength;
            if (zeroBytes > 0)
            {
                reader.ReadBytes(zeroBytes);
            }

            return bytes;
        }

        internal static char ReadChar(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(PadLength);
            return Encoding.ASCII.GetChars(bytes)[0];
        }

        internal static string ReadString(BinaryReader reader)
        {
            int index = 0;
            var done = false;
            var bytes = new byte[PadLength];
            while (!done)
            {
                var bytesRead = reader.Read(bytes, index, PadLength);
                if (bytesRead < PadLength)
                {
                    throw new InvalidOperationException("OSC string was not in a correct format.");
                }

                if ((bytesRead = Array.IndexOf<byte>(bytes, 0, index, PadLength)) >= 0)
                {
                    index = bytesRead;
                    done = true;
                }
                else
                {
                    index += PadLength;
                    if (index >= bytes.Length)
                    {
                        Array.Resize(ref bytes, bytes.Length * 2);
                    }
                }
            }

            return Encoding.ASCII.GetString(bytes, 0, index);
        }

        internal static Expression Address(Expression reader)
        {
            return Expression.Call(typeof(MessageParser), nameof(ReadString), null, reader);
        }

        internal static Expression Content(string typeTag, Expression reader)
        {
            if (string.IsNullOrEmpty(typeTag))
            {
                throw new ArgumentException("A valid type tag must be specified.", nameof(typeTag));
            }

            var stack = new Stack<List<Expression>>();
            var arguments = new List<Expression>();
            foreach (var tag in typeTag)
            {
                switch (tag)
                {
                    case TypeTag.Char:
                        arguments.Add(Expression.Call(typeof(MessageParser), nameof(ReadChar), null, reader));
                        break;
                    case TypeTag.TimeTag:
                        arguments.Add(Expression.Call(typeof(MessageParser), nameof(ReadTimeTag), null, reader));
                        break;
                    case TypeTag.Int64: arguments.Add(Expression.Call(reader, ReadInt64)); break;
                    case TypeTag.Int32: arguments.Add(Expression.Call(reader, ReadInt32)); break;
                    case TypeTag.Float: arguments.Add(Expression.Call(reader, ReadFloat)); break;
                    case TypeTag.Double: arguments.Add(Expression.Call(reader, ReadDouble)); break;
                    case TypeTag.Alternate:
                    case TypeTag.String: arguments.Add(Address(reader)); break;
                    case TypeTag.Blob:
                        var blobSize = Expression.Call(reader, ReadInt32);
                        arguments.Add(Expression.Call(typeof(MessageParser), nameof(ReadBlob), null, reader, blobSize));
                        break;
                    case TypeTag.ArrayBegin:
                        stack.Push(arguments);
                        arguments = new List<Expression>();
                        break;
                    case TypeTag.ArrayEnd:
                        var array = Arguments(arguments);
                        if (stack.Count == 0) throw new ArgumentException("Invalid OSC array declaration.", nameof(typeTag));
                        arguments = stack.Pop();
                        arguments.Add(array);
                        break;
                    default: throw new ArgumentException(string.Format("The type tag '{0}' is not supported.", tag), nameof(typeTag));
                }
            };

            if (stack.Count > 0)
            {
                throw new ArgumentException("Unexpected end of type tag. Check for missing array declaration brackets.", nameof(typeTag));
            }
            return Arguments(arguments);
        }

        static Expression Arguments(List<Expression> arguments)
        {
            if (arguments.Count == 0)
            {
                throw new ArgumentException("OSC arrays cannot be empty.", nameof(arguments));
            }

            if (arguments.Count > 8)
            {
                throw new ArgumentException("OSC messages or arrays with more than eight arguments are not supported.", nameof(arguments));
            }

            if (arguments.Count == 1) return arguments[0];
            else
            {
                var argumentTypes = arguments.Select(m => m.Type).ToArray();
                var tupleCreate = typeof(Tuple).GetMethods().First(
                    m => m.Name == nameof(Tuple.Create) &&
                    m.GetParameters().Length == arguments.Count)
                    .MakeGenericMethod(argumentTypes);
                return Expression.Call(tupleCreate, arguments);
            }
        }
    }
}
