using System;

namespace Bonsai.Osc
{
    /// <summary>
    /// Provides helper methods for converting to and from OSC time tags.
    /// </summary>
    public static class TimeTag
    {
        static readonly DateTimeOffset Epoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts an OSC time tag into a date time object relative to UTC.
        /// </summary>
        /// <param name="timeTag">The OSC time tag to convert.</param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/> value corresponding to the specified
        /// OSC time tag.
        /// </returns>
        public static DateTimeOffset ToTimestamp(ulong timeTag)
        {
            var seconds = (uint)((timeTag >> 32) & 0xFFFFFFFF);
            var fraction = (double)((uint)(timeTag & 0xFFFFFFFF)) / uint.MaxValue;
            var timeSpan = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * (seconds + fraction)));
            return Epoch + timeSpan;
        }

        /// <summary>
        /// Converts a date time object relative to UTC into an OSC time tag.
        /// </summary>
        /// <param name="timestamp">
        /// A <see cref="DateTimeOffset"/> specifying a point in time relative
        /// to UTC.
        /// </param>
        /// <returns>
        /// The OSC time tag corresponding to the specified <see cref="DateTimeOffset"/>.
        /// </returns>
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
