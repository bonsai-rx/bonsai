using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    [Description("Returns the directory information for the specified path string.")]
    public class GetDirectoryName : Transform<string, string>
    {
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetDirectoryName);
        }
    }
}
