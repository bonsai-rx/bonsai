using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.IO;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a non-generic base class for sinks that write all elements from the
    /// source sequence to a file.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(FileName))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class FileSink
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
        [Description("The name of the file on which to write the elements.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the suffix used to generate file names.
        /// </summary>
        [Description("The suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <see langword="true"/>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite { get; set; }
    }

    /// <summary>
    /// Provides a base class for sinks that write the elements from the input sequence
    /// into a file.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TWriter">The type of writer that should be used to write the elements.</typeparam>
    public abstract class FileSink<TSource, TWriter> : FileSink where TWriter : class, IDisposable
    {
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
        /// Writes all elements of an observable sequence to a file.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to write to the file.</param>
        /// <param name="selector">
        /// The transform function used to convert each element of the sequence into the type
        /// of inputs accepted by the file writer.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the elements to a file.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected IObservable<TElement> Process<TElement>(IObservable<TElement> source, Func<TElement, TSource> selector)
        {
            return Process(source, selector, FileName);
        }

        /// <summary>
        /// Writes all elements of an observable sequence to the specified file.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to write to the file.</param>
        /// <param name="selector">
        /// The transform function used to convert each element of the sequence into the type
        /// of inputs accepted by the file writer.
        /// </param>
        /// <param name="fileName">
        /// The name of the file on which to write the elements.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the elements to a file.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected IObservable<TElement> Process<TElement>(
            IObservable<TElement> source,
            Func<TElement, TSource> selector,
            string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid file path must be specified.");
            }

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
                PathHelper.EnsureDirectory(fileName);
                var filePath = PathHelper.AppendSuffix(fileName, Suffix);
                if (File.Exists(filePath) && !Overwrite)
                {
                    throw new IOException(string.Format("The file '{0}' already exists.", filePath));
                }

                var disposable = new WriterDisposable<TWriter>(Buffered);
                var process = source.Do(element =>
                {
                    Action writeTask = () =>
                    {
                        try
                        {
                            var input = selector(element);
                            var runningWriter = disposable.Writer;
                            if (runningWriter == null)
                            {
                                runningWriter = disposable.Writer = CreateWriter(filePath, input);
                            }

                            Write(runningWriter, input);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    };

                    disposable.Schedule(writeTask);
                }).SubscribeSafe(observer);

                return new CompositeDisposable(process, disposable);
            });
        }

        /// <summary>
        /// Writes all elements of an observable sequence to the specified file.
        /// </summary>
        /// <param name="source">The sequence of elements to write to the file.</param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the elements to a file.
        /// </returns>
        public virtual IObservable<TSource> Process(IObservable<TSource> source)
        {
            return Process(source, input => input);
        }
    }
}
