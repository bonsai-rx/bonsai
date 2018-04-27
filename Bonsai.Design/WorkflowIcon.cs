using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    [DebuggerDisplay("{Name}")]
    public abstract class WorkflowIcon
    {
        internal WorkflowIcon()
        {
        }

        public abstract string Name { get; }

        public abstract Stream GetStream();

        public static WorkflowIcon GetElementIcon(ExpressionBuilder builder)
        {
            return new ExpressionBuilderIcon(builder);
        }

        public static WorkflowIcon GetCategoryIcon(ElementCategory category)
        {
            return new ExpressionBuilderIcon(category);
        }
    }
}
