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

        public ReactiveWorkflow(IEnumerable<ILoadable> loadableElements, Expression output)
        {
            elements = loadableElements;
            Output = output;
        }

        public override IDisposable Load()
        {
            return new CompositeDisposable(elements.Select(element => element.Load()));
        }

        public Expression Output { get; private set; }
    }
}
