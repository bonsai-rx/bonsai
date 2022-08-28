using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a disposable reference to a video writer resource.
    /// </summary>
    public sealed class VideoWriterDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        internal VideoWriterDisposable(OpenCV.Net.VideoWriter writer, IDisposable disposable)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            resource = disposable;
        }

        /// <summary>
        /// Gets the reference to the disposable video writer instance.
        /// </summary>
        public OpenCV.Net.VideoWriter Writer { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the video writer has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return resource == null; }
        }

        /// <summary>
        /// Closes the video writer resource, which will flush all remaining data
        /// to disk and prevent further writes.
        /// </summary>
        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref resource, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
