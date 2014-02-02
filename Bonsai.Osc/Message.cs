using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    class Message
    {
        const char AddressSeparator = '/';
        int contentSize;
        int contentIndex;
        byte[] contents;
        MessagePattern[] addressParts;

        internal Message(byte[] packet, int index, int count)
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

        public BigEndianReader GetContents()
        {
            var stream = new MemoryStream(contents, contentIndex, contentSize, false);
            return new BigEndianReader(stream);
        }
    }
}
