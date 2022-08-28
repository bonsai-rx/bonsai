using OpenTK;
using System;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents errors that occur when compiling or linking shader programs.
    /// </summary>
    [Serializable]
    public class ShaderException : GraphicsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderException"/> class.
        /// </summary>
        public ShaderException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderException"/> class
        /// with the specified message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public ShaderException(string message)
            : base(message)
        {
        }
    }
}
