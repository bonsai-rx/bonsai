using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes the text representation of each element of the sequence to the
    /// output stream, followed by the current line terminator.
    /// </summary>
    [Description("Writes the text representation of each element of the sequence to the output stream, followed by the current line terminator.")]
    public class WriteLine : Sink
    {
        /// <summary>
        /// Writes the text representation of each element of an observable sequence to the standard
        /// output stream, followed by a line terminator.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the elements to write to the standard output stream.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence but
        /// where there is an additional side effect of writing the elements to the standard
        /// output stream.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input => Console.Out.WriteLineAsync(input.ToString()));
        }

        /// <summary>
        /// Writes the text representation of each element of an observable sequence to all the
        /// specified output streams, followed by a line terminator.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the elements to write to the active output streams.
        /// </param>
        /// <param name="writer">
        /// A sequence of <see cref="System.IO.TextWriter"/> objects on which to write the text
        /// representation of the elements of the <paramref name="source"/> sequence.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the elements to all active
        /// output streams.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<System.IO.TextWriter> writer)
        {
            return source.Publish(ps => Observable.Merge(
                writer.SelectMany(ws => ps.Do(input => ws.WriteLineAsync(input.ToString()))).IgnoreElements(),
                ps));
        }
    }
}
