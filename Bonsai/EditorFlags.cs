using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
