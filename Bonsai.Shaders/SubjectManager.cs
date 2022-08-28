using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Bonsai.Shaders
{
    class SubjectManager
    {
        static Tuple<ReplaySubject<ShaderWindow>, RefCountDisposable> windowSubject;
        static readonly object subjectLock = new object();

        internal static SubjectDisposable ReserveSubject()
        {
            lock (subjectLock)
            {
                if (windowSubject == null)
                {
                    var subject = new ReplaySubject<ShaderWindow>(2);
                    var dispose = Disposable.Create(() =>
                    {
                        subject.Dispose();
                        windowSubject = null;
                    });

                    var refCount = new RefCountDisposable(dispose);
                    windowSubject = Tuple.Create(subject, refCount);
                    return new SubjectDisposable(subject, refCount);
                }

                return new SubjectDisposable(windowSubject.Item1, windowSubject.Item2.GetDisposable());
            }
        }

        internal sealed class SubjectDisposable : IDisposable
        {
            IDisposable resource;

            public SubjectDisposable(ISubject<ShaderWindow> subject, IDisposable disposable)
            {
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
            }

            public ISubject<ShaderWindow> Subject { get; private set; }

            public void Dispose()
            {
                lock (subjectLock)
                {
                    if (resource != null)
                    {
                        resource.Dispose();
                        resource = null;
                    }
                }
            }
        }
    }
}
