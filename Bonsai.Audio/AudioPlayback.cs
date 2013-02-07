using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK;
using System.ComponentModel;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace Bonsai.Audio
{
    [Description("Plays the sequence of buffered samples to the specified audio output device.")]
    public class AudioPlayback : Sink<CvMat>
    {
        int source;
        AudioContext context;

        public AudioPlayback()
        {
            Frequency = 44100;
        }

        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The playback frequency (Hz) used by the output device.")]
        public int Frequency { get; set; }

        public override void Process(CvMat input)
        {
            var buffer = AL.GenBuffer();
            AL.BufferData(buffer, ALFormat.Mono16, input.Data, input.Rows * input.Step, Frequency);

            AL.SourceQueueBuffer(source, buffer);
            if (AL.GetSourceState(source) != ALSourceState.Playing)
            {
                AL.SourcePlay(source);
            }

            ClearBuffers(0);
        }

        void ClearBuffers(int input)
        {
            int[] freeBuffers;
            if (input == 0)
            {
                int processedBuffers;
                AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processedBuffers);
                if (processedBuffers == 0)
                    return;

                freeBuffers = AL.SourceUnqueueBuffers(source, processedBuffers);
            }
            else
            {
                freeBuffers = AL.SourceUnqueueBuffers(source, input);
            }

            AL.DeleteBuffers(freeBuffers);
        }

        public override IDisposable Load()
        {
            context = new AudioContext(DeviceName);
            source = AL.GenSource();
            return base.Load();
        }

        protected override void Unload()
        {
            int queuedBuffers;
            AL.GetSource(source, ALGetSourcei.BuffersQueued, out queuedBuffers);
            ClearBuffers(queuedBuffers);

            AL.DeleteSource(source);
            context.Dispose();
            base.Unload();
        }
    }
}
