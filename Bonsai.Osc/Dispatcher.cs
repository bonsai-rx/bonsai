using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    class Dispatcher
    {
        const byte MessageByte = 0x2F; // '/'
        const byte BundleByte =  0x23; // '#'
        const string BundleIdentifier = "#bundle";
        IObserver<Message> observer;
        EventLoopScheduler scheduler;

        public Dispatcher(IObserver<Message> observer, EventLoopScheduler scheduler)
        {
            if (observer == null)
            {
                throw new ArgumentNullException("observer");
            }

            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            this.observer = observer;
            this.scheduler = scheduler;
        }

        public void ProcessPacket(byte[] packet)
        {
            try { ProcessPacket(packet, 0, packet.Length); }
            catch (Exception e) { observer.OnError(e); }
        }

        private void ProcessPacket(byte[] packet, int index, int count)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }

            if (index < 0 || index >= packet.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var identifier = packet[index];
            if (identifier == MessageByte) ProcessMessage(packet, index, count);
            else if (identifier == BundleByte) ProcessBundle(packet, index, count);
            else throw new ArgumentException("An OSC packet must contain either a message or a bundle.", "packet");
        }

        private void ProcessMessage(byte[] packet, int index, int count)
        {
            var message = new Message(packet, index, count);
            observer.OnNext(message);
        }

        private void ProcessBundle(byte[] packet, int index, int count)
        {
            var currentIndex = index;
            var bundleId = ReadString(packet, ref currentIndex);
            if (bundleId != BundleIdentifier)
            {
                throw new ArgumentException(
                    string.Format("An OSC bundle must start with the OSC-string '{0}'.", BundleIdentifier),
                    "packet");
            }

            var timeTag = ReadTimeTag(packet, ref currentIndex);
            Action processElements = () =>
            {
                try
                {
                    while (currentIndex < index + count)
                    {
                        var elementSize = ReadInt32(packet, ref currentIndex);
                        ProcessPacket(packet, currentIndex, elementSize);
                        currentIndex += elementSize;
                    }
                }
                catch (Exception e) { observer.OnError(e); }
            };

            if (timeTag <= scheduler.Now) processElements();
            else scheduler.Schedule(timeTag, processElements);
        }

        private static int ReadInt32(byte[] packet, ref int index)
        {
            var value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(packet, index));
            index += sizeof(int);
            return value;
        }

        private static DateTimeOffset ReadTimeTag(byte[] packet, ref int index)
        {
            var timeTag = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(packet, index));
            index += sizeof(ulong);
            return TimeTag.ToTimestamp(timeTag);
        }

        internal static string ReadString(byte[] packet, ref int index)
        {
            const int PadLength = 4;
            var terminator = Array.IndexOf<byte>(packet, 0, index);
            if (terminator < 0)
            {
                throw new ArgumentException("OSC strings must be null terminated.", "packet");
            }

            var result = Encoding.ASCII.GetString(packet, index, terminator - index);
            index = terminator + PadLength - (terminator % PadLength);
            return result;
        }
    }
}
