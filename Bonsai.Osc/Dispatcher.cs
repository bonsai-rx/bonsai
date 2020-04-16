using System;
using System.Net;
using System.Reactive.Concurrency;
using System.Text;

namespace Bonsai.Osc
{
    class Dispatcher
    {
        const byte MessageByte = 0x2F; // '/'
        const byte BundleByte =  0x23; // '#'
        const string BundleIdentifier = "#bundle";
        readonly IObserver<Message> observer;
        readonly IScheduler scheduler;

        public Dispatcher(IObserver<Message> observer, IScheduler scheduler)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            this.observer = observer;
            this.scheduler = scheduler;
        }

        public void Process(byte[] array)
        {
            Process(array, 0, array.Length);
        }

        public void Process(ArraySegment<byte> buffer)
        {
            Process(buffer.Array, buffer.Offset, buffer.Count);
        }

        public void Process(byte[] buffer, int offset, int count)
        {
            try { ProcessPacket(buffer, offset, count); }
            catch (Exception e) { observer.OnError(e); }
        }

        private void ProcessPacket(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            var identifier = buffer[offset];
            if (identifier == MessageByte) ProcessMessage(buffer, offset, count);
            else if (identifier == BundleByte) ProcessBundle(buffer, offset, count);
            else throw new ArgumentException("An OSC packet must contain either a message or a bundle.", nameof(buffer));
        }

        private void ProcessMessage(byte[] buffer, int offset, int count)
        {
            var message = new Message(buffer, offset, count);
            observer.OnNext(message);
        }

        private void ProcessBundle(byte[] buffer, int offset, int count)
        {
            var currentIndex = offset;
            var bundleId = ReadString(buffer, ref currentIndex);
            if (bundleId != BundleIdentifier)
            {
                throw new ArgumentException(
                    string.Format("An OSC bundle must start with the OSC-string '{0}'.", BundleIdentifier),
                    nameof(buffer));
            }

            var timeTag = ReadTimeTag(buffer, ref currentIndex);
            Action processElements = () =>
            {
                try
                {
                    while (currentIndex < offset + count)
                    {
                        var elementSize = ReadInt32(buffer, ref currentIndex);
                        ProcessPacket(buffer, currentIndex, elementSize);
                        currentIndex += elementSize;
                    }
                }
                catch (Exception e) { observer.OnError(e); }
            };

            if (timeTag <= scheduler.Now) processElements();
            else scheduler.Schedule(timeTag, processElements);
        }

        private static int ReadInt32(byte[] buffer, ref int offset)
        {
            var value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            offset += sizeof(int);
            return value;
        }

        private static DateTimeOffset ReadTimeTag(byte[] buffer, ref int offset)
        {
            var timeTag = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, offset));
            offset += sizeof(ulong);
            return TimeTag.ToTimestamp(timeTag);
        }

        internal static string ReadString(byte[] buffer, ref int offset)
        {
            const int PadLength = 4;
            var terminator = Array.IndexOf<byte>(buffer, 0, offset);
            if (terminator < 0)
            {
                throw new ArgumentException("OSC strings must be null terminated.", nameof(buffer));
            }

            var result = Encoding.ASCII.GetString(buffer, offset, terminator - offset);
            offset = terminator + PadLength - (terminator % PadLength);
            return result;
        }
    }
}
