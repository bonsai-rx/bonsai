using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [Description("Writes each element of the input sequence followed by a line terminator to the output stream.")]
    public class WriteLine : Sink
    {
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input => Console.Out.WriteLineAsync(input.ToString()));
        }

        public IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<System.IO.TextWriter> writer)
        {
            return source.Publish(ps => Observable.Merge(
                writer.SelectMany(ws => ps.Do(input => ws.WriteLineAsync(input.ToString()))).IgnoreElements(),
                ps));
        }
    }
}
