using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class BuildContext
    {
        public BuildContext(ExpressionBuilder buildTarget)
        {
            BuildTarget = buildTarget;
        }

        public ExpressionBuilder BuildTarget { get; private set; }

        public Expression BuildResult { get; internal set; }
    }
}
