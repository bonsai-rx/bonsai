using System.Linq.Expressions;

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
