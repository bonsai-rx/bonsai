using NuGet;
using System;

namespace Bonsai.NuGet
{
    public class ConsoleLogger : ILogger
    {
        public static readonly ConsoleLogger Default = new ConsoleLogger();

        private ConsoleLogger()
        {
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            if (level == MessageLevel.Error) Console.Error.WriteLine(message, args);
            else Console.WriteLine(message, args);
        }

        public FileConflictResolution ResolveFileConflict(string message)
        {
            return FileConflictResolution.IgnoreAll;
        }
    }
}
