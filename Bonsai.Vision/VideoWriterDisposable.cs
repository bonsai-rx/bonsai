using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.Vision
{
    public sealed class VideoWriterDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public VideoWriterDisposable(OpenCV.Net.VideoWriter writer, IDisposable disposable)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            Writer = writer;
            resource = disposable;
        }

        public OpenCV.Net.VideoWriter Writer { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange<IDisposable>(ref resource, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
