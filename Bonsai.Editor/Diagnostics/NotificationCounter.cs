using System.Threading;

namespace Bonsai.Editor.Diagnostics
{
    internal struct NotificationCounter
    {
        private long count;
        private long lastCount;

        public void Increment()
        {
            Interlocked.Increment(ref count);
        }

        public long Read()
        {
            return Interlocked.Read(ref count);
        }

        public long ReadDelta(out long count)
        {
            count = Read();
            var delta = count - lastCount;
            lastCount = count;
            return delta;
        }
    }
}
