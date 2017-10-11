using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class IncludeContext : IBuildContext
    {
        IBuildContext parent;
        Expression buildResult;
        string includePath;

        public IncludeContext(IBuildContext parentContext, string path)
        {
            if (parentContext == null)
            {
                throw new ArgumentNullException("parentContext");
            }

            includePath = path;
            parent = parentContext;
            BuildTarget = parentContext.BuildTarget;
        }

        public string Path
        {
            get { return includePath; }
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
