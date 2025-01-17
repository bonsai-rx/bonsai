using System;

namespace Bonsai.Editor.GraphModel
{
    [Flags]
    enum WorkflowPathFlags
    {
        None = 0x0,
        ReadOnly = 0x1,
        Disabled = 0x2
    }
}
