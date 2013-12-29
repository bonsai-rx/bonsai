using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Bonsai.Expressions;

namespace Bonsai
{
    /// <summary>
    /// Represents errors that occur in expression builder workflows.
    /// </summary>
    [Serializable]
    public class WorkflowException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class.
        /// </summary>
        public WorkflowException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WorkflowException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public WorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class with
        /// a specified error message and a reference to the <see cref="ExpressionBuilder"/>
        /// that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="builder">
        /// The <see cref="ExpressionBuilder"/> that is the cause of the current exception, or
        /// a null reference (Nothing in Visual Basic) if no builder is specified.
        /// </param>
        public WorkflowException(string message, ExpressionBuilder builder)
            : base(message)
        {
            Builder = builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class with a
        /// specified error message and a reference to both the <see cref="ExpressionBuilder"/>
        /// and the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="builder">
        /// The <see cref="ExpressionBuilder"/> that is the cause of the current exception, or
        /// a null reference (Nothing in Visual Basic) if no builder is specified.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public WorkflowException(string message, ExpressionBuilder builder, Exception innerException)
            : base(message, innerException)
        {
            Builder = builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about
        /// the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about
        /// the source or destination.
        /// </param>
        protected WorkflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the <see cref="ExpressionBuilder"/> instance that was the cause for the exception.
        /// </summary>
        public ExpressionBuilder Builder { get; private set; }
    }
}
