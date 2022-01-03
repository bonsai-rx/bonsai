using OpenTK.Audio.OpenAL;
using System;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an audio buffer which can be used to store and manipulate audio data.
    /// </summary>
    public class Buffer : IDisposable
    {
        int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer"/> class.
        /// </summary>
        public Buffer()
        {
            AL.GenBuffers(1, out id);
        }

        /// <summary>
        /// Gets the name of the buffer. This is an OpenAL buffer reference which can be
        /// used to call audio manipulation functions.
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Buffer"/> class.
        /// </summary>
        public void Dispose()
        {
            AL.DeleteBuffers(1, ref id);
        }
    }
}
