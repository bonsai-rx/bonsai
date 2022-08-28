using NuGet.Common;
using System;

namespace Bonsai.NuGet
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(ILogMessage message)
        {
            Message = message;
        }

        public ILogMessage Message { get; private set; }
    }
}
