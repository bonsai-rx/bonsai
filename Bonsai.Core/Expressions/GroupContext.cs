using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class GroupContext : IBuildContext
    {
        IBuildContext parent;
        Expression buildResult;

        public GroupContext(IBuildContext parentContext)
        {
            if (parentContext == null)
            {
                throw new ArgumentNullException("parentContext");
            }

            parent = parentContext;
            BuildTarget = parentContext.BuildTarget;
        }

        public ExpressionBuilder BuildTarget { get; private set; }

        public Expression BuildResult
        {
            get { return buildResult; }
            set
            {
                buildResult = value;
                if (parent != null)
                {
                    parent.BuildResult = buildResult;
                }
            }
        }

        public IBuildContext ParentContext
        {
            get { return parent; }
        }

        public ParameterExpression AddVariable(string name, Expression expression)
        {
            return parent.AddVariable(name, expression);
        }

        public ParameterExpression GetVariable(string name)
        {
            return parent.GetVariable(name);
        }

        public Expression CloseContext(Expression source)
        {
            return source;
        }
    }
}
