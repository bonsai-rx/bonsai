using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Bonsai
{
    [Serializable]
    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException()
        {
        }

        public InvalidWorkflowException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidWorkflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
