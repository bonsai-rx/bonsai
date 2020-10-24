using System;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class GroupContext : IBuildContext
    {
        Expression buildResult;

        public GroupContext(IBuildContext parentContext)
        {
            ParentContext = parentContext ?? throw new ArgumentNullException(nameof(parentContext));
            BuildTarget = parentContext.BuildTarget;
        }

        public ExpressionBuilder BuildTarget { get; private set; }

        public Expression BuildResult
        {
            get { return buildResult; }
            set
            {
                buildResult = value;
                if (ParentContext != null)
                {
                    ParentContext.BuildResult = buildResult;
                }
            }
        }

        public IBuildContext ParentContext { get; }

        public ParameterExpression AddVariable(string name, Expression expression)
        {
            return ParentContext.AddVariable(name, expression);
        }

        public ParameterExpression GetVariable(string name)
        {
            return ParentContext.GetVariable(name);
        }

        public Expression CloseContext(Expression source)
        {
            return source;
        }
    }
}
