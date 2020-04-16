using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    [Description("Returns the file name and extension of the specified path string.")]
    public class GetFileName : Transform<string, string>
    {
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetFileName);
        }
    }
}
