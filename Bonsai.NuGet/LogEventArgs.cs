using NuGet;
using System;

namespace Bonsai.NuGet
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(MessageLevel level, string message, object[] args)
        {
            Level = level;
            Message = message;
            Args = args;
        }

        public MessageLevel Level { get; private set; }

        public string Message { get; private set; }

        public object[] Args { get; private set; }
    }
}
