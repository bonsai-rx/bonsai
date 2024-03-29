﻿using System;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a non-generic base class for sinks that write all elements from the
    /// source sequence to a named stream.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(Path))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class StreamSink
    {
        /// <summary>
        /// Gets or sets the identifier of the stream on which to write the elements.
        /// </summary>
        /// <remarks>
        /// If the identifier uses the named pipe prefix <c>\\.\pipe\</c>, a corresponding
        /// <see cref="NamedPipeServerStream"/> object is created; otherwise a regular
        /// <see cref="FileStream"/> is used.
        /// </remarks>
        [Description("The identifier of the named stream on which to write the elements.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the suffix that should be applied to the path before creating the writer.
        /// </summary>
        [Description("The suffix that should be applied to the path before creating the writer.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output path if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output path if it already exists.")]
        public bool Overwrite { get; set; }
    }

    /// <summary>
    /// Provides a base class for sinks that write the elements from the input sequence
    /// into a named stream (e.g. a named pipe).
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TWriter">The type of stream writer that should be used to write the elements.</typeparam>
    public abstract class StreamSink<TSource, TWriter> : StreamSink where TWriter : class, IDisposable
    {
        const string PipeServerPrefix = @"\\.\pipe\";

        /// <summary>
        /// When overridden in a derived class, creates the object that will be responsible
        /// for writing the input elements to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream on which the elements should be written.</param>
        /// <returns>The object that will be used to write elements into the stream.</returns>
        protected abstract TWriter CreateWriter(Stream stream);

        /// <summary>
        /// When overridden in a derived class, writes a new element using the specified writer.
        /// </summary>
        /// <param name="writer">The writer used to push elements into the stream.</param>
        /// <param name="input">The input element that should be pushed into the stream.</param>
        protected abstract void Write(TWriter writer, TSource input);

        static Stream CreateStream(string path, bool overwrite, CancellationToken cancellationToken)
        {
            const int MaxNumberOfServerInstances = 1;
            if (path.StartsWith(PipeServerPrefix))
            {
                var pipeName = path.Split(new[] { PipeServerPrefix }, StringSplitOptions.RemoveEmptyEntries).Single();
                var stream = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.Out,
                    MaxNumberOfServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
                try
                {
                    using (var cancellation = cancellationToken.Register(stream.Close))
                    {
                        stream.WaitForConnection();
                    }
                }
                catch { stream.Close(); throw; }
                return stream;
            }
            else return new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew);
        }

        /// <summary>
        /// Writes all elements of an observable sequence to a stream.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to write.</param>
        /// <param name="selector">
        /// The transform function used to convert each element of the sequence into the type
        /// of inputs accepted by the stream writer.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to a stream.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">A valid path must be specified.</exception>
        protected IObservable<TElement> Process<TElement>(IObservable<TElement> source, Func<TElement, TSource> selector)
        {
            return Process(source, selector, Path);
        }

        /// <summary>
        /// Writes all elements of an observable sequence into the specified stream.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to write.</param>
        /// <param name="selector">
        /// The transform function used to convert each element of the sequence into the type
        /// of inputs accepted by the stream writer.
        /// </param>
        /// <param name="path">
        /// The identifier of the stream on which to write the elements.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to the named stream.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">A valid path must be specified.</exception>
        protected IObservable<TElement> Process<TElement>(
            IObservable<TElement> source,
            Func<TElement, TSource> selector,
            string path)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return Observable.Create<TElement>(observer =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("A valid path must be specified.");
                }

                var cancellationSource = new CancellationTokenSource();
                var cancel = Disposable.Create(cancellationSource.Cancel);
                var disposable = new WriterDisposable<TWriter>();
                disposable.Schedule(() =>
                {
                    Stream stream = null;
                    try
                    {
                        if (!path.StartsWith(@"\\")) PathHelper.EnsureDirectory(path);
                        var streamPath = PathHelper.AppendSuffix(path, Suffix);
                        stream = CreateStream(streamPath, Overwrite, cancellationSource.Token);
                        disposable.Writer = CreateWriter(stream);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        if (stream != null) stream.Close();
                    }
                });

                var process = source.Do(input =>
                {
                    disposable.Schedule(() =>
                    {
                        if (disposable.Writer != null)
                        {
                            try { Write(disposable.Writer, selector(input)); }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                            }
                        }
                    });
                }).SubscribeSafe(observer);

                return new CompositeDisposable(process, disposable, cancel, cancellationSource);
            });
        }

        /// <summary>
        /// Writes all elements of an observable sequence to the specified stream.
        /// </summary>
        /// <param name="source">The sequence of elements to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the elements to a stream.
        /// </returns>
        public virtual IObservable<TSource> Process(IObservable<TSource> source)
        {
            return Process(source, input => input);
        }
    }
}
