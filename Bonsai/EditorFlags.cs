using System;

namespace Bonsai
{
    [Flags]
    enum EditorFlags
    {
        None = 0x0,
        UpdatesAvailable = 0x1,
        DebugScripts = 0x2
    }
}
