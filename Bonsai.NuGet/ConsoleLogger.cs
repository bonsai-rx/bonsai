using NuGet.Common;
using System;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class ConsoleLogger : LoggerBase
    {
        public static readonly ConsoleLogger Default = new ConsoleLogger();

        private ConsoleLogger()
        {
        }

        public override void Log(ILogMessage message)
        {
            if (message.Level == LogLevel.Error) Console.Error.WriteLine(message);
            else Console.WriteLine(message);
        }

        public override Task LogAsync(ILogMessage message)
        {
            return Task.Factory.StartNew(() => Log(message));
        }
    }
}
