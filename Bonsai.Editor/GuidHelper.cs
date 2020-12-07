using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Bonsai.Editor
{
    static class GuidHelper
    {
        public static Guid GetProcessGuid()
        {
            using var process = Process.GetCurrentProcess();
            var processId = process.Id;
            var startTime = process.StartTime;
            var seed = processId.ToString(CultureInfo.InvariantCulture) + startTime.Ticks.ToString(CultureInfo.InvariantCulture);
            using var cryptoProvider = MD5.Create();
            var seedBytes = Encoding.Default.GetBytes(seed);
            var hash = cryptoProvider.ComputeHash(seedBytes);
            return new Guid(hash);
        }
    }
}
