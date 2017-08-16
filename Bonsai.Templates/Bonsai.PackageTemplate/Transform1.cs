using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// TODO: replace this with the transform input and output types.
using TSource = System.String;
using TResult = System.String;

namespace $safeprojectname$
{
    public class Transform1 : Transform<TSource, TResult>
    {
        public override IObservable<TResult> Process(IObservable<TSource> source)
        {
            return source.Select(input =>
            {
                // TODO: process the input object and return the result.
                throw new NotImplementedException();
                return default(TResult);
            });
        }
    }
}
