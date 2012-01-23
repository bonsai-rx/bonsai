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
        IEnumerable<LoadableElement> elements;

        public ReactiveWorkflow(IEnumerable<LoadableElement> loadableElements, IEnumerable<Expression> connections)
        {
            elements = loadableElements;
            Connections = connections;
        }

        public override IDisposable Load()
        {
            return new CompositeDisposable(elements.Select(element => element.Load()));
        }

        public IEnumerable<Expression> Connections { get; private set; }
    }
}
