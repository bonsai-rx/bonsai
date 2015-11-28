using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    static class MessageBuilder
    {
        const int PadLength = 4;
        const string ASCIIEncodingName = "ASCII";
        const string WriteMethodName = "Write";
        const string GetBytesMethodName = "GetBytes";
        const string PadBytesMethodName = "PadBytes";
        const string WriteBlobMethodName = "WriteBlob";
        const string ArrayLengthProperty = "Length";
        const string BlockCopyMethodName = "BlockCopy";
        const string FromTimestampMethodName = "FromTimestamp";
        static readonly MethodInfo GetBytes = typeof(Encoding).GetMethod(GetBytesMethodName, new[] { typeof(string) });
        static readonly MethodInfo GetCharBytes = typeof(Encoding).GetMethod(GetBytesMethodName, new[] { typeof(char[]) });
        static readonly MethodInfo WriteBytes = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(byte[]) });
        static readonly MethodInfo WriteInt32 = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(int) });
        static readonly MethodInfo WriteInt64 = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(long) });
        static readonly MethodInfo WriteUInt64 = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(ulong) });
        static readonly MethodInfo WriteFloat = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(float) });
        static readonly MethodInfo WriteDouble = typeof(BigEndianWriter).GetMethod(WriteMethodName, new[] { typeof(double) });
        static readonly ConstructorInfo DateTimeOffsetConstructor = typeof(DateTimeOffset).GetConstructor(new[] { typeof(DateTime) });

        public const string AddressSeparator = "/";
        const string TypeTagSeparator = ",";

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            if (type.IsInterface)
            {
                members = members.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)));
            }
            return members.OrderBy(member => member.MetadataToken);
        }

        static byte[] PadBytes(byte[] value, int zeroPad)
        {
            var excess = (value.Length + zeroPad) % PadLength;
            if (excess > 0 || zeroPad > 0)
            {
                if (excess == 0) excess = PadLength;
                Array.Resize(ref value, value.Length + PadLength - excess + zeroPad);
            }

            return value;
        }

        static void WriteBlob(BigEndianWriter writer, byte[] buffer)
        {
            writer.Write(buffer.Length);
            writer.Write(buffer);

            var zeroPad = buffer.Length % PadLength;
            if (zeroPad > 0)
            {
                var padBytes = new byte[zeroPad];
                writer.Write(padBytes);
            }
        }

        static Expression CreateMessageBuilder(Expression parameter, StringBuilder typeTagBuilder, Expression writer)
        {
            var type = parameter.Type;
            var typeCode = type == typeof(DateTimeOffset) ? TypeCode.DateTime : Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Char:
                    typeTagBuilder.Append(TypeTag.Char);
                    var charPad = Expression.Constant(0);
                    var charEncoding = Expression.Property(null, typeof(Encoding), ASCIIEncodingName);
                    parameter = Expression.NewArrayInit(typeof(char), parameter);
                    parameter = Expression.Call(charEncoding, GetCharBytes, parameter);
                    parameter = Expression.Call(typeof(MessageBuilder), PadBytesMethodName, null, parameter, charPad);
                    return Expression.Call(writer, WriteBytes, parameter);
                case TypeCode.DateTime:
                    typeTagBuilder.Append(TypeTag.TimeTag);
                    if (type != typeof(DateTimeOffset))
                    {
                        parameter = Expression.New(DateTimeOffsetConstructor, parameter);
                    }
                    parameter = Expression.Call(typeof(TimeTag), FromTimestampMethodName, null, parameter);
                    return Expression.Call(writer, WriteUInt64, parameter);
                case TypeCode.Decimal:
                case TypeCode.Double:
                    if (typeCode != TypeCode.Double)
                    {
                        parameter = Expression.Convert(parameter, typeof(double));
                    }
                    typeTagBuilder.Append(TypeTag.Double);
                    return Expression.Call(writer, WriteDouble, parameter);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    typeTagBuilder.Append(TypeTag.Nil);
                    break;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    if (typeCode != TypeCode.Int32)
                    {
                        parameter = Expression.Convert(parameter, typeof(int));
                    }
                    typeTagBuilder.Append(TypeTag.Int32);
                    return Expression.Call(writer, WriteInt32, parameter);
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    if (typeCode != TypeCode.Int64)
                    {
                        parameter = Expression.Convert(parameter, typeof(long));
                    }
                    typeTagBuilder.Append(TypeTag.Int64);
                    return Expression.Call(writer, WriteInt64, parameter);
                case TypeCode.Single:
                    typeTagBuilder.Append(TypeTag.Float);
                    return Expression.Call(writer, WriteFloat, parameter);
                case TypeCode.String:
                    typeTagBuilder.Append(TypeTag.String);
                    var stringPad = Expression.Constant(1);
                    var stringEncoding = Expression.Property(null, typeof(Encoding), ASCIIEncodingName);
                    parameter = Expression.Call(stringEncoding, GetBytes, parameter);
                    parameter = Expression.Call(typeof(MessageBuilder), PadBytesMethodName, null, parameter, stringPad);
                    return Expression.Call(writer, WriteBytes, parameter);
                case TypeCode.Object:
                default:
                    if (type.IsArray)
                    {
                        typeTagBuilder.Append(TypeTag.Blob);
                        var elementType = type.GetElementType();
                        if (!elementType.IsValueType)
                        {
                            throw new ArgumentException("OSC-blob arrays of reference types are not supported.", "argument");
                        }

                        Expression blobAssignment;
                        var blobVariable = Expression.Variable(typeof(byte[]));
                        if (elementType != typeof(byte))
                        {
                            var offset = Expression.Constant(0);
                            var elementSize = Expression.Constant(Marshal.SizeOf(elementType));
                            var arrayLength = Expression.Property(parameter, ArrayLengthProperty);
                            var blobBounds = Expression.Multiply(arrayLength, elementSize);
                            var blobArray = Expression.NewArrayBounds(typeof(byte), blobBounds);
                            var blockCopy = Expression.Call(typeof(Buffer), BlockCopyMethodName, null, parameter, offset, blobVariable, offset, blobBounds);
                            blobAssignment = Expression.Assign(blobVariable, blobArray);
                            blobAssignment = Expression.Block(blobAssignment, blockCopy);
                        }
                        else blobAssignment = Expression.Assign(blobVariable, parameter);
                        var blobSize = Expression.Property(blobVariable, ArrayLengthProperty);

                        return Expression.Block(
                            new[] { blobVariable },
                            blobAssignment,
                            Expression.Call(typeof(MessageBuilder), WriteBlobMethodName, null, writer, blobVariable));
                    }
                    else
                    {
                        var members = GetDataMembers(type);
                        return Expression.Block(members.Select(member =>
                        {
                            var memberAccess = Expression.MakeMemberAccess(parameter, member);
                            return CreateMessageBuilder(memberAccess, typeTagBuilder, writer);
                        }));
                    }
            }

            return parameter;
        }

        public static Expression Message(string address, Expression parameter, Expression writer)
        {
            if (string.IsNullOrEmpty(address) || !address.StartsWith(AddressSeparator))
            {
                throw new ArgumentException(
                    string.Format("The OSC Address Pattern cannot be null and must begin with a '{0}' character.", AddressSeparator),
                    "address");
            }

            var addressBytes = Encoding.ASCII.GetBytes(address);
            var typeTagBuilder = new StringBuilder();
            typeTagBuilder.Append(TypeTagSeparator);
            var messageBuilder = CreateMessageBuilder(parameter, typeTagBuilder, writer);
            var typeTag = typeTagBuilder.ToString();
            var typeTagBytes = Encoding.ASCII.GetBytes(typeTag);

            addressBytes = PadBytes(addressBytes, 1);
            typeTagBytes = PadBytes(typeTagBytes, 1);
            var addressExpression = Expression.Constant(addressBytes);
            var typeTagExpression = Expression.Constant(typeTagBytes);
            return Expression.Block(
                Expression.Call(writer, WriteBytes, addressExpression),
                Expression.Call(writer, WriteBytes, typeTagExpression),
                messageBuilder);
        }
    }
}
