using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Bonsai.Expressions;

namespace Bonsai
{
    [Serializable]
    public class WorkflowBuildException : Exception
    {
        public WorkflowBuildException()
        {
        }

        public WorkflowBuildException(string message)
            : base(message)
        {
        }

        public WorkflowBuildException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowBuildException(string message, ExpressionBuilder builder, Exception innerException)
            : base(message, innerException)
        {
            Builder = builder;
        }

        protected WorkflowBuildException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ExpressionBuilder Builder { get; private set; }
    }
}
