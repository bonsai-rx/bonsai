using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public sealed class ShaderDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public ShaderDisposable(Shader shader, IDisposable disposable)
        {
            if (shader == null)
            {
                throw new ArgumentNullException("shader");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Shader = shader;
            resource = disposable;
        }

        public Shader Shader { get; private set; }

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
