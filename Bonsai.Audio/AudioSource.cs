using OpenTK;
using OpenTK.Audio.OpenAL;
using System;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents a source of spatialized audio which can be used to define and control the
    /// audio landscape surrounding the listener.
    /// </summary>
    public class AudioSource : IDisposable
    {
        int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioSource"/> class.
        /// </summary>
        public AudioSource()
        {
            id = AL.GenSource();
        }

        /// <summary>
        /// Gets the name of the audio source. This is an OpenAL buffer reference which can be
        /// used to call audio source manipulation functions.
        /// </summary>
        public int Id
        {
            get { return id; }
        }

        /// <summary>
        /// Gets or sets the volume amplification applied to the audio source.
        /// </summary>
        /// <remarks>
        /// Each division by 2 equals an attenuation of -6 dB, and each multiplication
        /// by 2 an amplification by +6 dB. A value of 1.0 means the source is unchanged,
        /// and zero is interpreted as zero volume.
        /// </remarks>
        public float Gain
        {
            get
            {
                AL.GetSource(id, ALSourcef.Gain, out float value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourcef.Gain, value);
            }
        }

        /// <summary>
        /// Gets or sets the pitch to be applied to the audio source.
        /// </summary>
        public float Pitch
        {
            get
            {
                AL.GetSource(id, ALSourcef.Pitch, out float value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourcef.Pitch, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source uses coordinates
        /// relative to the listener.
        /// </summary>
        public bool Relative
        {
            get
            {
                AL.GetSource(id, ALSourceb.SourceRelative, out bool value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourceb.SourceRelative, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is looping.
        /// </summary>
        public bool Looping
        {
            get
            {
                AL.GetSource(id, ALSourceb.Looping, out bool value);
                return value;
            }
            set
            {
                AL.Source(id, ALSourceb.Looping, value);
            }
        }

        /// <summary>
        /// Gets or sets the direction vector of the audio source.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                AL.GetSource(id, ALSource3f.Direction, out Vector3 value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Direction, ref value);
            }
        }

        /// <summary>
        /// Gets or sets the location of the audio source in three-dimensional space.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                AL.GetSource(id, ALSource3f.Position, out Vector3 value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Position, ref value);
            }
        }

        /// <summary>
        /// Gets or sets the velocity of the audio source in three-dimensional space.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                AL.GetSource(id, ALSource3f.Velocity, out Vector3 value);
                return value;
            }
            set
            {
                AL.Source(id, ALSource3f.Velocity, ref value);
            }
        }

        /// <summary>
        /// Gets information about the current source state.
        /// </summary>
        public ALSourceState State
        {
            get { return AL.GetSourceState(id); }
        }

        /// <summary>
        /// Stops the source and sets its state to <see cref="ALSourceState.Initial"/>.
        /// </summary>
        public void Rewind()
        {
            AL.SourceRewind(id);
        }

        /// <summary>
        /// Plays, replays, or resumes the source and sets its state to
        /// <see cref="ALSourceState.Playing"/>. If the source is already playing,
        /// the source will restart at the beginning.
        /// </summary>
        public void Play()
        {
            AL.SourcePlay(id);
        }

        /// <summary>
        /// Pauses the source and sets its state to <see cref="ALSourceState.Paused"/>.
        /// </summary>
        public void Pause()
        {
            AL.SourcePause(id);
        }

        /// <summary>
        /// Stops the source and sets its state to <see cref="ALSourceState.Stopped"/>.
        /// </summary>
        public void Stop()
        {
            AL.SourceStop(id);
        }

        internal void ClearBuffers(int input)
        {
            int[] freeBuffers;
            if (input == 0)
            {
                AL.GetSource(id, ALGetSourcei.BuffersProcessed, out int processedBuffers);
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

        /// <summary>
        /// Releases all resources used by the <see cref="AudioSource"/> class.
        /// </summary>
        public void Dispose()
        {
            if (id != 0)
            {
                AL.GetSource(id, ALGetSourcei.BuffersQueued, out int queuedBuffers);
                ClearBuffers(queuedBuffers);

                AL.DeleteSource(id);
                id = 0;
            }
        }
    }
}
