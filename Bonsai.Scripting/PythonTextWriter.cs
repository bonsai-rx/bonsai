using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Drawing.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Bonsai.IO;

namespace Bonsai.Scripting
{
    [Description("A Python script used to write individual elements of the input sequence to a text file.")]
    public class PythonTextWriter : PythonSink
    {
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design",
                "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Description("The name of the output file.")]
        public string FileName { get; set; }

        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        [Description("The optional suffix used to create file names.")]
        public PathSuffix Suffix { get; set; }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source, Action<TSource> action, ScriptScope scope)
        {
            Task writerTask = null;
            return Observable.Using(
                () =>
                {
                    var fileName = PathHelper.AppendSuffix(FileName, Suffix);
                    writerTask = new Task(() => { });
                    writerTask.Start();
                    var disposable = new CompositeDisposable();
                    disposable.Add(Disposable.Create(writerTask.Wait));
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        var writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                        scope.Engine.Runtime.IO.SetOutput(writer.BaseStream, writer);
                        disposable.Add(writer);
                    }
                    return disposable;
                },
                resource => source.Do(xs =>
                {
                    writerTask = writerTask.ContinueWith(task => action(xs));
                }));
        }
    }
}
