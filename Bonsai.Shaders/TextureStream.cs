using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureStream : Texture, ITextureSequence
    {
        readonly int capacity;
        readonly IEnumerator<IplImage> frames;
        readonly PixelInternalFormat? pixelFormat;
        IEnumerator<IplImage> preloaded;
        Task<IplImage> queryFrame;

        public TextureStream(IEnumerator<IplImage> source, PixelInternalFormat? internalFormat, int bufferLength)
        {
            queryFrame = Task.FromResult(default(IplImage));
            pixelFormat = internalFormat;
            capacity = bufferLength;
            frames = source;
        }

        public bool Loop { get; set; }

        public double PlaybackRate { get; set; }

        public bool MoveNext()
        {
            var result = preloaded.MoveNext();
            if (!result) return false;
            GL.BindTexture(TextureTarget.Texture2D, Id);
            TextureHelper.UpdateTexture(TextureTarget.Texture2D, pixelFormat, preloaded.Current);
            return true;
        }

        public void Reset()
        {
            Loop = false;
            if (preloaded != null)
            {
                preloaded.Dispose();
                queryFrame = queryFrame.ContinueWith(task =>
                {
                    frames.Reset();
                    return default(IplImage);
                });
            }

            preloaded = GetPreloadedFrames(frames, capacity);
        }

        Task<IplImage> GetNextFrame(IEnumerator<IplImage> frames, CancellationToken cancellationToken)
        {
            return queryFrame = queryFrame.ContinueWith(task =>
            {
                if (cancellationToken.IsCancellationRequested) return null;
                if (!frames.MoveNext())
                {
                    if (!Loop) return null;
                    frames.Reset();
                    frames.MoveNext();
                }

                return frames.Current;
            });
        }

        IEnumerator<IplImage> GetFrameEnumerator(
            IEnumerator<IplImage> frames,
            Queue<Task<IplImage>> taskBuffer,
            CancellationTokenSource cancellation)
        {
            try
            {
                while (true)
                {
                    var nextFrame = taskBuffer.Dequeue();
                    if (!nextFrame.IsCompleted) nextFrame.Wait(cancellation.Token);
                    if (nextFrame.Result == null) break;
                    taskBuffer.Enqueue(GetNextFrame(frames, cancellation.Token));
                    yield return nextFrame.Result;
                }
            }
            finally
            {
                cancellation.Cancel();
            }
        }

        IEnumerator<IplImage> GetPreloadedFrames(IEnumerator<IplImage> frames, int bufferLength)
        {
            var taskBuffer = new Queue<Task<IplImage>>(bufferLength);
            var cancellation = new CancellationTokenSource();
            while (taskBuffer.Count < bufferLength)
            {
                taskBuffer.Enqueue(GetNextFrame(frames, cancellation.Token));
            }
            return GetFrameEnumerator(frames, taskBuffer, cancellation);
        }

        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (preloaded != null)
                {
                    preloaded.Dispose();
                    queryFrame = queryFrame.ContinueWith(task =>
                    {
                        frames.Dispose();
                        return default(IplImage);
                    });
                }
            }
            base.Dispose(disposing);
        }
    }
}
