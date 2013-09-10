using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Bonsai.Expressions;

namespace Bonsai
{
    [Serializable]
    public class WorkflowRuntimeException : WorkflowException
    {
        public WorkflowRuntimeException()
        {
        }

        public WorkflowRuntimeException(string message)
            : base(message)
        {
        }

        public WorkflowRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowRuntimeException(string message, ExpressionBuilder builder)
            : base(message, builder)
        {
        }

        public WorkflowRuntimeException(string message, ExpressionBuilder builder, Exception innerException)
            : base(message, builder, innerException)
        {
        }

        protected WorkflowRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
