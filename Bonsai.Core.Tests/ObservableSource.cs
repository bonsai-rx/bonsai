using System;

namespace Bonsai.Core.Tests
{
    internal class ObservableSource<T> : Source<T>
    {
        readonly IObservable<T> source;

        public ObservableSource(IObservable<T> source)
        {
            this.source = source;
        }

        public override IObservable<T> Generate()
        {
            return source;
        }
    }
}
