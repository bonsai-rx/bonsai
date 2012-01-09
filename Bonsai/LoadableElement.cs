using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Bonsai
{
    public abstract class LoadableElement : ILoadable
    {
        public virtual IDisposable Load()
        {
            return Disposable.Create(Unload);
        }

        protected virtual void Unload()
        {
        }
    }
}
