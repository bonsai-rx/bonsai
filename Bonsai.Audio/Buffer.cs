using OpenTK.Audio.OpenAL;
using System;

namespace Bonsai.Audio
{
    public class Buffer : IDisposable
    {
        int id;

        public Buffer()
        {
            AL.GenBuffers(1, out id);
        }

        public int Id
        {
            get { return id; }
        }

        public void Dispose()
        {
            AL.DeleteBuffers(1, ref id);
        }
    }
}
