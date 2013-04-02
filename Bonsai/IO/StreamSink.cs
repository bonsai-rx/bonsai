using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;

namespace Bonsai.IO
{
    public abstract class StreamSink<TSource, TWriter> : Sink<TSource> where TWriter : class, IDisposable
    {
        const string PipeServerPrefix = @"\\.\pipe\";
        Task<TWriter> writerTask;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Path { get; set; }

        public PathSuffix Suffix { get; set; }

        protected abstract TWriter CreateWriter(Stream stream);

        protected abstract void Write(TWriter writer, TSource input);

        public override void Process(TSource input)
        {
            if (writerTask == null) return;
            writerTask = writerTask.ContinueWith(task =>
            {
                Write(task.Result, input);
                return task.Result;
            });
        }

        static Stream CreateStream(string path)
        {
            if (path.StartsWith(PipeServerPrefix))
            {
                var pipeName = path.Split(new[] { PipeServerPrefix }, StringSplitOptions.RemoveEmptyEntries).Single();
                var stream = new NamedPipeServerStream(pipeName, PipeDirection.Out);
                stream.WaitForConnection();
                return stream;
            }
            else return new FileStream(path, FileMode.Create);
        }

        public override IDisposable Load()
        {
            var path = Path;
            if (!string.IsNullOrEmpty(path))
            {
                writerTask = new Task<TWriter>(() =>
                {
                    if (!path.StartsWith(@"\\")) PathHelper.EnsureDirectory(path);
                    path = PathHelper.AppendSuffix(path, Suffix);
                    var stream = CreateStream(path);
                    return CreateWriter(stream);
                });
                writerTask.Start();
            }

            return base.Load();
        }

        protected override void Unload()
        {
            writerTask.ContinueWith(task =>
            {
                task.Result.Dispose();
            });

            writerTask = null;
            base.Unload();
        }
    }
}
