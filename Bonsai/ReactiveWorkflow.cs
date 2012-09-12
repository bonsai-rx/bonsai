using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Disposables;

namespace Bonsai
{
    public class ReactiveWorkflow : LoadableElement
    {
        IEnumerable<ILoadable> elements;

        public ReactiveWorkflow(IEnumerable<ILoadable> loadableElements, IList<Expression> connections)
        {
            elements = loadableElements;
            Connections = connections;
        }

        public override IDisposable Load()
        {
            return new CompositeDisposable(elements.Select(element => element.Load()));
        }

        public IList<Expression> Connections { get; private set; }
    }
}
