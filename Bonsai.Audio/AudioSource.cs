using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class AudioSource : IDisposable
    {
        int id;

        public AudioSource()
        {
            id = AL.GenSource();
        }

        public int Id
        {
            get { return id; }
        }

        public bool Relative
        {
            get
            {
                bool value;
                AL.GetSource(id, ALSourceb.SourceRelative, out value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourceb.SourceRelative, value);
            }
        }

        public bool Looping
        {
            get
            {
                bool value;
                AL.GetSource(id, ALSourceb.Looping, out value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourceb.Looping, value);
            }
        }

        public Vector3 Direction
        {
            get
            {
                Vector3 value;
                AL.GetSource(id, ALSource3f.Direction, out value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Direction, ref value);
            }
        }

        public Vector3 Position
        {
            get
            {
                Vector3 value;
                AL.GetSource(id, ALSource3f.Position, out value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Position, ref value);
            }
        }

        public Vector3 Velocity
        {
            get
            {
                Vector3 value;
                AL.GetSource(id, ALSource3f.Velocity, out value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Velocity, ref value);
            }
        }

        public ALSourceState State
        {
            get { return AL.GetSourceState(id); }
        }

        internal void ClearBuffers(int input)
        {
            int[] freeBuffers;
            if (input == 0)
            {
                int processedBuffers;
                AL.GetSource(id, ALGetSourcei.BuffersProcessed, out processedBuffers);
                if (processedBuffers == 0)
                    return;

                freeBuffers = AL.SourceUnqueueBuffers(id, processedBuffers);
            }
            else
            {
                freeBuffers = AL.SourceUnqueueBuffers(id, input);
            }

            AL.DeleteBuffers(freeBuffers);
        }

        public void Dispose()
        {
            if (id != 0)
            {
                int queuedBuffers;
                AL.GetSource(id, ALGetSourcei.BuffersQueued, out queuedBuffers);
                ClearBuffers(queuedBuffers);

                AL.DeleteSource(id);
                id = 0;
            }
        }
    }
}
