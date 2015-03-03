using Bonsai.Osc.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    public sealed class Message
    {
        const char AddressSeparator = '/';
        int contentSize;
        int contentIndex;
        byte[] contents;
        MessagePattern[] addressParts;

        public Message(byte[] packet, int index, int count)
        {
            contentIndex = index;
            Address = Dispatcher.ReadString(packet, ref contentIndex);
            TypeTag = Dispatcher.ReadString(packet, ref contentIndex);
            contents = packet;
            contentSize = count - (contentIndex - index);
            addressParts = Array.ConvertAll(
                Address.Split(AddressSeparator),
                pattern => new MessagePattern(pattern));
        }

        public string Address { get; private set; }

        public string TypeTag { get; private set; }

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

        public Stream GetStream()
        {
            return new MemoryStream(contents, contentIndex, contentSize, false);
        }

        public IEnumerable GetContents()
        {
            var chars = TypeTag.ToArray();
            var activeArrays = new Stack<ArrayList>();
            using (var reader = new BigEndianReader(GetStream()))
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
            var array = obj as object[];
            if (array != null)
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
            else
            {
                var blob = obj as byte[];
                if (blob != null) return string.Format("{0}[{1}]", typeof(byte).Name, blob.Length);
                else return obj.ToString();
            }
        }

        public override string ToString()
        {
            var contentValues = GetContents().Cast<object>().Select(obj => ToString(obj)).ToArray();
            var contents = string.Join(CultureInfo.InvariantCulture.TextInfo.ListSeparator, contentValues);
            return string.Format("Address: {0} TypeTag: {1} Contents: {{{2}}}", Address, TypeTag, contents);
        }
    }
}
