using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a base class for sinks that write the elements from the input sequence
    /// into a file.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TWriter">The type of writer that should be used to write the elements.</typeparam>
    public abstract class FileSink<TSource, TWriter> : Sink<TSource> where TWriter : class, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSink{TSource, TWriter}"/> class.
        /// </summary>
        protected FileSink()
        {
            Buffered = true;
        }

        /// <summary>
        /// Gets or sets the name of the file on which to write the elements.
        /// </summary>
        [Description("The name of the output file.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the suffix that should be applied to the path before creating the writer.
        /// </summary>
        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <b>true</b>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered { get; set; }

        /// <summary>
        /// When overridden in a derived class, creates the writer over the specified
        /// <paramref name="fileName"/> that will be responsible for handling the input elements.
        /// </summary>
        /// <param name="fileName">The name of the file on which the elements should be written.</param>
        /// <param name="input">The first input element that needs to be pushed into the file.</param>
        /// <returns>The writer that will be used to push elements into the file.</returns>
        protected abstract TWriter CreateWriter(string fileName, TSource input);

        /// <summary>
        /// When overridden in a derived class, writes a new element into the specified writer.
        /// </summary>
        /// <param name="writer">The writer that is used to push elements into the file.</param>
        /// <param name="input">The input element that should be pushed into the file.</param>
        protected abstract void Write(TWriter writer, TSource input);

        /// <summary>
        /// Writes all elements of an observable sequence into the specified file.
        /// </summary>
        /// <param name="source">The source sequence for which to write elements.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to a file.
        /// </returns>
        public override IObservable<TSource> Process(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                Task writerTask = null;
                TWriter writer = null;

                if (Buffered)
                {
                    writerTask = new Task(() => { });
                    writerTask.Start();
                }

                var close = Disposable.Create(() =>
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
                });

                var process = source.Do(input =>
                {
                    Action writeTask = () =>
                    {
                        try
                        {
                            var runningWriter = writer;
                            if (runningWriter == null)
                            {
                                var fileName = FileName;
                                if (string.IsNullOrEmpty(fileName)) return;

                                PathHelper.EnsureDirectory(fileName);
                                fileName = PathHelper.AppendSuffix(fileName, Suffix);
                                runningWriter = writer = CreateWriter(fileName, input);
                            }

                            Write(runningWriter, input);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    };

                    if (writerTask == null) writeTask();
                    else writerTask = writerTask.ContinueWith(task => writeTask());
                }).Subscribe(observer);

                return new CompositeDisposable(process, close);
            });
        }
    }
}
