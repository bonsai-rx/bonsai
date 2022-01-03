using Bonsai.Resources;
using OpenTK.Audio;
using System;

namespace Bonsai.Audio
{
    /// <summary>
    /// Manages the lifetime of an audio context and its associated resources.
    /// </summary>
    public class AudioContextManager : IDisposable
    {
        readonly AudioContext context;
        readonly ResourceManager resourceManager;

        internal AudioContextManager(string deviceName, int sampleRate, int refresh)
        {
            context = new AudioContext(deviceName, sampleRate, refresh);
            resourceManager = new ResourceManager();
        }

        /// <summary>
        /// Gets the audio context associated with any loaded audio resources.
        /// </summary>
        public AudioContext AudioContext
        {
            get { return context; }
        }

        /// <summary>
        /// Gets the resource manager storing all the resources associated with this audio context.
        /// </summary>
        public ResourceManager ResourceManager
        {
            get { return resourceManager; }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AudioContextManager"/> class.
        /// </summary>
        public void Dispose()
        {
            resourceManager.Dispose();
            context.Dispose();
        }
    }
}
