﻿using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Bonsai.Osc
{
    static class MessageBuilder
    {
        const int PadLength = 4;
        static readonly MethodInfo GetBytes = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(string) });
        static readonly MethodInfo GetCharBytes = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(char[]) });
        static readonly MethodInfo WriteBytes = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(byte[]) });
        static readonly MethodInfo WriteInt32 = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(int) });
        static readonly MethodInfo WriteInt64 = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(long) });
        static readonly MethodInfo WriteUInt64 = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(ulong) });
        static readonly MethodInfo WriteFloat = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(float) });
        static readonly MethodInfo WriteDouble = typeof(BigEndianWriter).GetMethod(nameof(BigEndianWriter.Write), new[] { typeof(double) });
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

        static void WriteMessage(BigEndianWriter writer, Message message)
        {
            var offset = message.Address.Length + 1;
            var excess = offset % PadLength;
            if (excess != 0) offset += PadLength - excess;
            var array = message.Buffer.Array;
            var count = message.Buffer.Count - offset;
            writer.Write(array, offset, count);
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
                    var charEncoding = Expression.Property(null, typeof(Encoding), nameof(Encoding.ASCII));
                    parameter = Expression.NewArrayInit(typeof(char), parameter);
                    parameter = Expression.Call(charEncoding, GetCharBytes, parameter);
                    parameter = Expression.Call(typeof(MessageBuilder), nameof(MessageBuilder.PadBytes), null, parameter, charPad);
                    return Expression.Call(writer, WriteBytes, parameter);
                case TypeCode.DateTime:
                    typeTagBuilder.Append(TypeTag.TimeTag);
                    if (type != typeof(DateTimeOffset))
                    {
                        parameter = Expression.New(DateTimeOffsetConstructor, parameter);
                    }
                    parameter = Expression.Call(typeof(TimeTag), nameof(TimeTag.FromTimestamp), null, parameter);
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
                    var stringEncoding = Expression.Property(null, typeof(Encoding), nameof(Encoding.ASCII));
                    parameter = Expression.Call(stringEncoding, GetBytes, parameter);
                    parameter = Expression.Call(typeof(MessageBuilder), nameof(MessageBuilder.PadBytes), null, parameter, stringPad);
                    return Expression.Call(writer, WriteBytes, parameter);
                case TypeCode.Object:
                default:
                    if (type.IsArray)
                    {
                        typeTagBuilder.Append(TypeTag.Blob);
                        var elementType = type.GetElementType();
                        if (!elementType.IsValueType)
                        {
                            throw new ArgumentException("OSC-blob arrays of reference types are not supported.", nameof(parameter));
                        }

                        Expression blobAssignment;
                        var blobVariable = Expression.Variable(typeof(byte[]));
                        if (elementType != typeof(byte))
                        {
                            var offset = Expression.Constant(0);
                            var elementSize = Expression.Constant(Marshal.SizeOf(elementType));
                            var arrayLength = Expression.Property(parameter, nameof(Array.Length));
                            var blobBounds = Expression.Multiply(arrayLength, elementSize);
                            var blobArray = Expression.NewArrayBounds(typeof(byte), blobBounds);
                            var blockCopy = Expression.Call(typeof(Buffer), nameof(Buffer.BlockCopy), null, parameter, offset, blobVariable, offset, blobBounds);
                            blobAssignment = Expression.Assign(blobVariable, blobArray);
                            blobAssignment = Expression.Block(blobAssignment, blockCopy);
                        }
                        else blobAssignment = Expression.Assign(blobVariable, parameter);
                        var blobSize = Expression.Property(blobVariable, nameof(Array.Length));

                        return Expression.Block(
                            new[] { blobVariable },
                            blobAssignment,
                            Expression.Call(typeof(MessageBuilder), nameof(MessageBuilder.WriteBlob), null, writer, blobVariable));
                    }
                    else
                    {
                        var members = GetDataMembers(type);
                        return Expression.Block(members.Select(member =>
                        {
                            var memberAccess = Expression.MakeMemberAccess(parameter, member);
                            if (memberAccess.Type == type)
                            {
                                throw new ArgumentException("Recursive data types are not supported.", nameof(parameter));
                            }

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
                    nameof(address));
            }

            var addressBytes = Encoding.ASCII.GetBytes(address);
            addressBytes = PadBytes(addressBytes, 1);
            var addressExpression = Expression.Constant(addressBytes);
            if (parameter.Type == typeof(Message))
            {
                return Expression.Block(
                    Expression.Call(writer, WriteBytes, addressExpression),
                    Expression.Call(typeof(MessageBuilder), nameof(WriteMessage), null, writer, parameter));
            }

            var typeTagBuilder = new StringBuilder();
            typeTagBuilder.Append(TypeTagSeparator);
            var messageBuilder = CreateMessageBuilder(parameter, typeTagBuilder, writer);
            var typeTag = typeTagBuilder.ToString();
            var typeTagBytes = Encoding.ASCII.GetBytes(typeTag);

            typeTagBytes = PadBytes(typeTagBytes, 1);
            var typeTagExpression = Expression.Constant(typeTagBytes);
            return Expression.Block(
                Expression.Call(writer, WriteBytes, addressExpression),
                Expression.Call(writer, WriteBytes, typeTagExpression),
                messageBuilder);
        }
    }
}
