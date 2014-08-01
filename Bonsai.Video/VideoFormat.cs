using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai.Video
{
    public class VideoFormat
    {
        public Size FrameSize;
        public int AverageFrameRate;
        public int MaximumFrameRate;
        public int BitCount;

        public VideoFormat()
        {
        }

        public VideoFormat(VideoCapabilities capabilities)
        {
            FrameSize = capabilities.FrameSize;
            AverageFrameRate = capabilities.AverageFrameRate;
            MaximumFrameRate = capabilities.MaximumFrameRate;
            BitCount = capabilities.BitCount;
        }

        internal bool Equals(VideoCapabilities capabilities)
        {
            return FrameSize == capabilities.FrameSize
                && AverageFrameRate == capabilities.AverageFrameRate
                && MaximumFrameRate == capabilities.MaximumFrameRate
                && BitCount == capabilities.BitCount;
        }

        internal static VideoFormat Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var regex = new Regex(@"([0-9]+)x([0-9]+)@([0-9]+)bpp \(([0-9]+)-?([0-9]+)? fps\)");
            var match = regex.Match(s);
            if (!match.Success)
            {
                throw new ArgumentException("The specified video format string is invalid.");
            }

            var averageFrameRate = int.Parse(match.Groups[4].Value);
            return new VideoFormat
            {
                FrameSize = new Size(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)),
                AverageFrameRate = averageFrameRate,
                MaximumFrameRate = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : averageFrameRate,
                BitCount = int.Parse(match.Groups[3].Value)
            };
        }

        public override string ToString()
        {
            const string BaseFormat = "{0}x{1}@{2}bpp ";
            var format = BaseFormat + (AverageFrameRate == MaximumFrameRate ? "({3} fps)" : "({3}-{4} fps)");
            return string.Format(CultureInfo.InvariantCulture, format, FrameSize.Width, FrameSize.Height, BitCount, AverageFrameRate, MaximumFrameRate);
        }
    }
}
