using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    interface IBuildContext
    {
        ExpressionBuilder BuildTarget { get; }

        Expression BuildResult { get; set; }

        IBuildContext ParentContext { get; }

        ParameterExpression AddVariable(string name, Expression expression);

        ParameterExpression GetVariable(string name);

        Expression CloseContext(Expression source);
    }
}
