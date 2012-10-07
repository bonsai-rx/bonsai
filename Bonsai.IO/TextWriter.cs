using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    public class TextWriter : Sink<object>
    {
        Task writerTask;
        StreamWriter writer;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design",
                "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public bool Append { get; set; }

        public PathSuffix Suffix { get; set; }

        public override void Process(object input)
        {
            writerTask = writerTask.ContinueWith(task =>
            {
                writer.WriteLine(input);
            });
        }

        public override IDisposable Load()
        {
            var fileName = PathHelper.AppendSuffix(FileName, Suffix);
            writerTask = new Task(() => { });
            writerTask.Start();
            var disposable = new CompositeDisposable();
            disposable.Add(Disposable.Create(writerTask.Wait));
            if (!string.IsNullOrEmpty(FileName))
            {
                writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                disposable.Add(writer);
            }
            return disposable;
        }
    }
}
