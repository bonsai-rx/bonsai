using System;

namespace Bonsai.Editor
{
    internal sealed class DocumentationException : Exception
    {
        public DocumentationException()
            : base()
        { }

        public DocumentationException(string message)
            : base(message)
        { }

        public DocumentationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
