using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Bonsai.IO
{
    public abstract class FileSink<TSource, TWriter> : Sink<TSource> where TWriter : class, IDisposable
    {
        Task writerTask;
        TWriter writer;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public PathSuffix Suffix { get; set; }

        protected abstract TWriter CreateWriter(string fileName, TSource input);

        protected abstract void Write(TWriter writer, TSource input);

        public override void Process(TSource input)
        {
            if (writerTask == null) return;

            var runningWriter = writer;
            writerTask = writerTask.ContinueWith(task =>
            {
                if (runningWriter == null)
                {
                    PathHelper.EnsureDirectory(FileName);
                    var fileName = PathHelper.AppendSuffix(FileName, Suffix);
                    runningWriter = writer = CreateWriter(fileName, input);
                }

                Write(runningWriter, input);
            });
        }

        public override IDisposable Load()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                writerTask = new Task(() => { });
                writerTask.Start();
            }

            return base.Load();
        }

        protected override void Unload()
        {
            var closingWriter = writer;
            writerTask.ContinueWith(task =>
            {
                if (closingWriter != null)
                {
                    closingWriter.Dispose();
                }
            });

            writerTask = null;
            writer = null;
            base.Unload();
        }
    }
}
