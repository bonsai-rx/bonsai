using NuGet;
using System;

namespace Bonsai.NuGet
{
    public class EventLogger : ILogger
    {
        public event EventHandler<LogEventArgs> Log;

        void ILogger.Log(MessageLevel level, string message, params object[] args)
        {
            var handler = Log;
            if (handler != null)
            {
                handler(this, new LogEventArgs(level, message, args));
            }
        }

        FileConflictResolution IFileConflictResolver.ResolveFileConflict(string message)
        {
            return FileConflictResolution.IgnoreAll;
        }
    }
}
