﻿using System;
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

        protected FileSink()
        {
            Buffered = true;
        }

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public PathSuffix Suffix { get; set; }

        public bool Buffered { get; set; }

        protected abstract TWriter CreateWriter(string fileName, TSource input);

        protected abstract void Write(TWriter writer, TSource input);

        public override void Process(TSource input)
        {
            var runningWriter = writer;
            Action writeTask = () =>
            {
                if (runningWriter == null)
                {
                    var fileName = FileName;
                    if (string.IsNullOrEmpty(fileName)) return;

                    PathHelper.EnsureDirectory(fileName);
                    fileName = PathHelper.AppendSuffix(fileName, Suffix);
                    runningWriter = writer = CreateWriter(fileName, input);
                }

                Write(runningWriter, input);
            };

            if (writerTask == null) writeTask();
            else writerTask = writerTask.ContinueWith(task => writeTask());
        }

        public override IDisposable Load()
        {
            if (Buffered)
            {
                writerTask = new Task(() => { });
                writerTask.Start();
            }

            return base.Load();
        }

        protected override void Unload()
        {
            var closingWriter = writer;
            Action closingTask = () =>
            {
                if (closingWriter != null)
                {
                    closingWriter.Dispose();
                }
            };

            if (writerTask == null) closingTask();
            else writerTask.ContinueWith(task => closingTask());
            writerTask = null;
            writer = null;
            base.Unload();
        }
    }
}
