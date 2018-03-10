using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    interface IRequireBuildContext
    {
        IBuildContext BuildContext { get; set; }
    }
}
