using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Bonsai.Expressions;

namespace Bonsai
{
    [Serializable]
    public class WorkflowException : Exception
    {
        public WorkflowException()
        {
        }

        public WorkflowException(string message)
            : base(message)
        {
        }

        public WorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowException(string message, ExpressionBuilder builder, Exception innerException)
            : base(message, innerException)
        {
            Builder = builder;
        }

        protected WorkflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ExpressionBuilder Builder { get; private set; }
    }
}
