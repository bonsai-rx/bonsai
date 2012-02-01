using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;
using System.Xml.Serialization;

namespace Bonsai
{
    [XmlType(Namespace = Constants.XmlNamespace)]
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
