using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that gets the value of an environment variable for the current process.
    /// </summary>
    ///
    [Description("Returns the value of an environment variable for the current process.")]
    public class GetEnvironmentVariable : Source<string>
    {
        /// <summary>
        /// Gets or sets the name of the environment variable to query the value of.
        /// </summary>
        [Description("The name of the environment variable to query the value of.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the value of the specified environment variable for the current process
        /// and returns it through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing the value of the specified environment variable. The
        /// value will be <see langword="null"/> if the environment variable is not found.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Environment.GetEnvironmentVariable(Name));
        }

        /// <summary>
        /// Gets the value of the specified environment variable for the current process
        /// whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to get the value of the environment variable.
        /// </param>
        /// <returns>
        /// A sequence containing the current values of the specified environment variable.
        /// The value may be <see langword="null"/> if the environment variable is not found.
        /// </returns>
        public IObservable<string> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(_ => Environment.GetEnvironmentVariable(Name));
        }
    }
}
