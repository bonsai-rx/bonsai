using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [Description("Reads a line of characters asynchronously from the input stream.")]
    public class ReadLine : Source<string>
    {
        IObservable<string> Generate(TextReader reader)
        {
            return Observable
                .FromAsync(reader.ReadLineAsync)
                .Repeat()
                .TakeWhile(text => text != null);
        }

        public override IObservable<string> Generate()
        {
            return Observable.Using(
                () => new StreamReader(Console.OpenStandardInput()),
                reader => Generate(reader));
        }

        public IObservable<string> Generate(IObservable<TextReader> source)
        {
            return source.SelectMany(reader => Generate(reader));
        }
    }
}
