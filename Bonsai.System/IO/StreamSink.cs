﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a base class for sinks that write the elements from the input sequence
    /// into a named stream (e.g. a named pipe).
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TWriter">The type of stream writer that should be used to write the elements.</typeparam>
    public abstract class StreamSink<TSource, TWriter> : Sink<TSource> where TWriter : class, IDisposable
    {
        const string PipeServerPrefix = @"\\.\pipe\";

        /// <summary>
        /// Gets or sets the identifier of the named stream on which to write the elements.
        /// </summary>
        [Description("The name of the output data path.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the suffix that should be applied to the path before creating the writer.
        /// </summary>
        [Description("The optional suffix used to generate path names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output if it already exists.
        /// </summary>
        [Description("Indicates whether the output should be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        /// <summary>
        /// When overridden in a derived class, creates the writer over the specified <see cref="Stream"/>
        /// instance that will be responsible for handling the input elements.
        /// </summary>
        /// <param name="stream">The stream on which the elements should be written.</param>
        /// <returns>The writer that will be used to push elements into the stream.</returns>
        protected abstract TWriter CreateWriter(Stream stream);

        /// <summary>
        /// When overridden in a derived class, writes a new element into the specified writer.
        /// </summary>
        /// <param name="writer">The writer that is used to push elements into the stream.</param>
        /// <param name="input">The input element that should be pushed into the stream.</param>
        protected abstract void Write(TWriter writer, TSource input);

        static Stream CreateStream(string path, bool overwrite)
        {
            if (path.StartsWith(PipeServerPrefix))
            {
                var pipeName = path.Split(new[] { PipeServerPrefix }, StringSplitOptions.RemoveEmptyEntries).Single();
                var stream = new NamedPipeServerStream(pipeName, PipeDirection.Out);
                try { stream.WaitForConnection(); }
                catch { stream.Close(); throw; }
                return stream;
            }
            else return new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew);
        }

        /// <summary>
        /// Writes all elements of an observable sequence into the specified stream.
        /// </summary>
        /// <param name="source">The source sequence for which to write elements.</param>
        /// <param name="selector">
        /// The transform function used to convert each element of the sequence into the type
        /// of inputs accepted by the stream writer.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to a stream.
        /// </returns>
        protected IObservable<TElement> Process<TElement>(IObservable<TElement> source, Func<TElement, TSource> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            return Observable.Create<TElement>(observer =>
            {
                var path = Path;
                Task<TWriter> writerTask = null;
                if (!string.IsNullOrEmpty(path))
                {
                    writerTask = new Task<TWriter>(() =>
                    {
                        Stream stream = null;
                        try
                        {
                            if (!path.StartsWith(@"\\")) PathHelper.EnsureDirectory(path);
                            path = PathHelper.AppendSuffix(path, Suffix);
                            stream = CreateStream(path, Overwrite);
                            return CreateWriter(stream);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            if (stream != null) stream.Close();
                            return null;
                        }
                    });
                    writerTask.Start();
                }

                var close = Disposable.Create(() =>
                {
                    if (writerTask != null)
                    {
                        writerTask.ContinueWith(task =>
                        {
                            var writer = task.Result;
                            if (writer != null) writer.Dispose();
                        });
                    }
                });

                var process = source.Do(input =>
                {
                    if (writerTask == null) return;
                    writerTask = writerTask.ContinueWith(task =>
                    {
                        if (task.Result != null)
                        {
                            try { Write(task.Result, selector(input)); }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                            }
                        }

                        return task.Result;
                    });
                }).SubscribeSafe(observer);

                return new CompositeDisposable(process, close);
            });
        }

        /// <summary>
        /// Writes all elements of an observable sequence into the specified stream.
        /// </summary>
        /// <param name="source">The source sequence for which to write elements.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to a stream.
        /// </returns>
        public override IObservable<TSource> Process(IObservable<TSource> source)
        {
            return Process(source, input => input);
        }
    }
}
