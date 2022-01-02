using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that reads lines of characters asynchronously from the input stream.
    /// </summary>
    [Description("Reads lines of characters asynchronously from the input stream.")]
    public class ReadLine : Source<string>
    {
        IObservable<string> Generate(TextReader reader)
        {
            return Observable
                .FromAsync(reader.ReadLineAsync)
                .Repeat()
                .TakeWhile(text => text != null);
        }

        /// <summary>
        /// Reads lines of characters asynchronously from the standard input stream.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing each of the lines
        /// read from the standard input stream, or <see langword="null"/> if all
        /// of the characters have been read.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Using(
                () => new StreamReader(Console.OpenStandardInput()),
                reader => Generate(reader));
        }

        /// <summary>
        /// Reads lines of characters asynchronously from a <see cref="TextReader"/> object.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="TextReader"/> objects from which to read lines.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing the lines read from each
        /// of the <see cref="TextReader"/> objects in the original sequence, or
        /// <see langword="null"/> if all of the characters have been read.
        /// </returns>
        public IObservable<string> Generate(IObservable<TextReader> source)
        {
            return source.SelectMany(reader => Generate(reader));
        }
    }
}
