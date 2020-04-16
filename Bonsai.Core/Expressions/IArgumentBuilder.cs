using Bonsai.Dag;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    interface IArgumentBuilder
    {
        bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument);
    }
}
