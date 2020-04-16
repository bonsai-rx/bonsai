using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    [Description("Returns the file name of the specified path string without the extension.")]
    public class GetFileNameWithoutExtension : Transform<string, string>
    {
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetFileNameWithoutExtension);
        }
    }
}
