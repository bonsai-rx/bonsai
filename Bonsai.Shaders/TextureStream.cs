using Bonsai.Reactive;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureStream : Texture, ITextureSequence
    {
        bool loop;
        readonly int capacity;
        readonly IEnumerator<IplImage> frames;
        readonly PixelInternalFormat? pixelFormat;
        IEnumerator<ElementIndex<IplImage>> preloaded;
        Task<ElementIndex<IplImage>> queryFrame;

        public TextureStream(IEnumerator<IplImage> source, PixelInternalFormat? internalFormat, int bufferLength)
        {
            queryFrame = Task.FromResult(default(ElementIndex<IplImage>));
            preloaded = GetPreloadedFrames(source, bufferLength);
            pixelFormat = internalFormat;
            capacity = bufferLength;
            frames = source;
        }

        public double PlaybackRate { get; set; }

        Task<ElementIndex<IplImage>> GetNextFrame(IEnumerator<IplImage> frames, CancellationToken cancellationToken)
        {
            return queryFrame = queryFrame.ContinueWith(task =>
            {
                var index = task.Result.Index + 1;
                if (cancellationToken.IsCancellationRequested) return default;
                if (!frames.MoveNext())
                {
                    if (!loop) return default;
                    frames.Reset();
                    frames.MoveNext();
                    index = 0;
                }

                return new ElementIndex<IplImage>(frames.Current, index);
            });
        }

        IEnumerator<ElementIndex<IplImage>> GetFrameEnumerator(
            IEnumerator<IplImage> frames,
            Queue<Task<ElementIndex<IplImage>>> taskBuffer,
            CancellationTokenSource cancellation)
        {
            try
            {
                while (true)
                {
                    var nextFrame = taskBuffer.Dequeue();
                    if (!nextFrame.IsCompleted) nextFrame.Wait(cancellation.Token);
                    if (nextFrame.Result.Value == null) break;
                    taskBuffer.Enqueue(GetNextFrame(frames, cancellation.Token));
                    yield return nextFrame.Result;
                }
            }
            finally
            {
                cancellation.Cancel();
            }
        }

        IEnumerator<ElementIndex<IplImage>> GetPreloadedFrames(IEnumerator<IplImage> frames, int bufferLength)
        {
            var taskBuffer = new Queue<Task<ElementIndex<IplImage>>>(bufferLength);
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
                        return default(ElementIndex<IplImage>);
                    });
                }
            }
            base.Dispose(disposing);
        }

        public IEnumerator<ElementIndex<Texture>> GetEnumerator(bool loop)
        {
            try
            {
                this.loop = loop;
                while (preloaded != null)
                {
                    var result = preloaded.MoveNext();
                    if (!result) yield break;

                    var current = preloaded.Current;
                    GL.BindTexture(TextureTarget.Texture2D, Id);
                    TextureHelper.UpdateTexture(TextureTarget.Texture2D, pixelFormat, current.Value);
                    yield return new ElementIndex<Texture>(this, current.Index);
                }
            }
            finally
            {
                this.loop = false;
                if (preloaded != null)
                {
                    preloaded.Dispose();
                    queryFrame = queryFrame.ContinueWith(task =>
                    {
                        frames.Reset();
                        return default(ElementIndex<IplImage>);
                    });
                }

                preloaded = GetPreloadedFrames(frames, capacity);
            }
        }
    }
}
