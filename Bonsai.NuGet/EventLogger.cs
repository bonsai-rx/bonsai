using NuGet.Common;
using System;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class EventLogger : LoggerBase
    {
        public event EventHandler<LogEventArgs> LogMessage;

        public override void Log(ILogMessage message)
        {
            LogMessage?.Invoke(this, new LogEventArgs(message));
        }

        public override Task LogAsync(ILogMessage message)
        {
            return Task.Factory.StartNew(() => Log(message));
        }
    }
}
