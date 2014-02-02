using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    public static class TimeTag
    {
        static readonly DateTimeOffset Epoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTimeOffset ToTimestamp(ulong timeTag)
        {
            var seconds = (uint)((timeTag >> 32) & 0xFFFFFFFF);
            var fraction = (double)((uint)(timeTag & 0xFFFFFFFF)) / uint.MaxValue;
            var timeSpan = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * (seconds + fraction)));
            return Epoch + timeSpan;
        }

        public static ulong FromTimestamp(DateTimeOffset timestamp)
        {
            var timeSpan = timestamp - Epoch;
            var totalSeconds = timeSpan.TotalSeconds;
            var seconds = (uint)totalSeconds;
            var fraction = (uint)((totalSeconds - seconds) * uint.MaxValue);
            return ((ulong)seconds) << 32 | (ulong)fraction;
        }
    }
}
